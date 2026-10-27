import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router'
import { api } from '../api/client'
import {
  shipmentStatuses,
  shipmentTypes,
  weightCategories,
  weightCategoryLabels,
  type PagedResult,
  type PostOffice,
  type Shipment,
  type ShipmentQuery,
} from '../api/types'
import Pagination from '../components/Pagination'
import ShipmentFilters from '../components/ShipmentFilters'
import StatusBadge from '../components/StatusBadge'

const pageSize = 10

/** Returns the value only if it is one of the allowed options (the URL can contain anything). */
function oneOf<T extends string>(options: readonly T[], value: string | null): T | undefined {
  return options.find((option) => option === value)
}

/** Filters and page live in the URL, so they survive going to the edit form and back. */
function readQuery(params: URLSearchParams): ShipmentQuery {
  return {
    page: Number(params.get('page')) || 1,
    pageSize,
    shipmentNumber: params.get('shipmentNumber') ?? undefined,
    status: oneOf(shipmentStatuses, params.get('status')),
    postOfficeId: Number(params.get('postOfficeId')) || undefined,
    weightCategory: oneOf(weightCategories, params.get('weightCategory')),
    type: oneOf(shipmentTypes, params.get('type')),
  }
}

function formatPostOffice(postOffice: PostOffice) {
  return `${postOffice.zipCode} ${postOffice.name}`
}

export default function ShipmentsPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const query = readQuery(searchParams)

  const [postOffices, setPostOffices] = useState<PostOffice[]>([])
  const [result, setResult] = useState<PagedResult<Shipment> | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [reloadKey, setReloadKey] = useState(0) // bump to reload after an action

  // We are loading while the last finished request is not the current one.
  const queryString = searchParams.toString()
  const requestKey = `${queryString}#${reloadKey}`
  const [finishedKey, setFinishedKey] = useState<string | null>(null)
  const loading = finishedKey !== requestKey

  useEffect(() => {
    api.getPostOffices().then(setPostOffices, (e: Error) => setError(e.message))
  }, [])

  // Re-fetch whenever the URL query changes. The "ignore" flag drops responses
  // that arrive after a newer request has started (e.g. while typing in the search box).
  useEffect(() => {
    let ignore = false
    api
      .getShipments(readQuery(new URLSearchParams(queryString)))
      .then((data) => {
        if (ignore) return
        setResult(data)
        setError(null)
      })
      .catch((e: Error) => {
        if (!ignore) setError(e.message)
      })
      .finally(() => {
        if (!ignore) setFinishedKey(`${queryString}#${reloadKey}`)
      })
    return () => {
      ignore = true
    }
  }, [queryString, reloadKey])

  function updateQuery(changes: Partial<ShipmentQuery>) {
    const next: ShipmentQuery = { ...query, page: 1, ...changes } // a filter change goes back to page 1
    const params = new URLSearchParams()
    for (const [key, value] of Object.entries(next)) {
      if (key !== 'pageSize' && value !== undefined && value !== '') params.set(key, String(value))
    }
    setSearchParams(params)
  }

  async function runAction(action: () => Promise<unknown>) {
    try {
      await action()
      setReloadKey((key) => key + 1)
    } catch (e) {
      setError((e as Error).message)
    }
  }

  function handleDelete(shipment: Shipment) {
    if (!confirm(`Delete shipment ${shipment.shipmentNumber}?`)) return
    void runAction(async () => {
      await api.deleteShipment(shipment.id)
      // If we deleted the last row on this page, step back one page.
      if (result?.items.length === 1 && query.page > 1) updateQuery({ page: query.page - 1 })
    })
  }

  return (
    <>
      <div className="page-header">
        <h1>Shipments</h1>
        <Link to="/shipments/new" className="button primary">
          New shipment
        </Link>
      </div>

      <ShipmentFilters query={query} postOffices={postOffices} onChange={updateQuery} />

      {error && <p className="error">{error}</p>}

      {result === null ? (
        loading && <p className="muted">Loading…</p>
      ) : result.items.length === 0 ? (
        <p className="muted">No shipments match these filters.</p>
      ) : (
        <>
          <table style={{ opacity: loading ? 0.6 : 1 }}>
            <thead>
              <tr>
                <th>Number</th>
                <th>Type</th>
                <th>Weight</th>
                <th>Status</th>
                <th>Origin</th>
                <th>Destination</th>
                <th>Current location</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {result.items.map((shipment) => (
                <tr key={shipment.id}>
                  <td className="number">{shipment.shipmentNumber}</td>
                  <td>{shipment.type}</td>
                  <td>
                    {shipment.weightKg} kg{' '}
                    <span className="muted">({weightCategoryLabels[shipment.weightCategory]})</span>
                  </td>
                  <td>
                    <StatusBadge status={shipment.status} />
                  </td>
                  <td>{formatPostOffice(shipment.origin)}</td>
                  <td>{formatPostOffice(shipment.destination)}</td>
                  <td>{formatPostOffice(shipment.currentPostOffice)}</td>
                  <td className="actions">
                    <Link to={`/shipments/${shipment.id}/edit`} className="button">
                      Edit
                    </Link>
                    <button
                      onClick={() => void runAction(() => api.advanceShipmentStatus(shipment.id))}
                      disabled={shipment.status === 'Delivered'}
                      title="Move to the next status"
                    >
                      Advance
                    </button>
                    <button className="danger" onClick={() => handleDelete(shipment)}>
                      Delete
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <Pagination
            page={query.page} // from the URL, so fast double-clicks don't use a stale page
            pageSize={query.pageSize}
            totalCount={result.totalCount}
            onPageChange={(page) => updateQuery({ page })}
          />
        </>
      )}
    </>
  )
}
