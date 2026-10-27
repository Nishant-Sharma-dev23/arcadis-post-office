import { useEffect, useState, type FormEvent } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router'
import { api } from '../api/client'
import { shipmentTypes, type PostOffice, type Shipment, type ShipmentType } from '../api/types'
import StatusBadge from '../components/StatusBadge'

// Form inputs hold strings; they are converted to numbers on submit.
interface FormState {
  type: ShipmentType
  weightKg: string
  originPostOfficeId: string
  destinationPostOfficeId: string
}

const emptyForm: FormState = { type: 'Package', weightKg: '', originPostOfficeId: '', destinationPostOfficeId: '' }

/** Used for both /shipments/new and /shipments/:id/edit. */
export default function ShipmentFormPage() {
  const { id } = useParams()
  const shipmentId = id ? Number(id) : null
  const navigate = useNavigate()
  const location = useLocation()

  const [postOffices, setPostOffices] = useState<PostOffice[]>([])
  const [shipment, setShipment] = useState<Shipment | null>(null)
  const [form, setForm] = useState<FormState>(emptyForm)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function load() {
      try {
        setPostOffices(await api.getPostOffices())
        if (shipmentId !== null) {
          const existing = await api.getShipment(shipmentId)
          setShipment(existing)
          setForm({
            type: existing.type,
            weightKg: String(existing.weightKg),
            originPostOfficeId: String(existing.origin.id),
            destinationPostOfficeId: String(existing.destination.id),
          })
        }
      } catch (e) {
        setError((e as Error).message)
      } finally {
        setLoading(false)
      }
    }
    void load()
  }, [shipmentId])

  // Go back to the list the user came from (keeps its filters); fall back to the list itself.
  function goBack() {
    if (location.key !== 'default') navigate(-1)
    else navigate('/shipments')
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if (form.originPostOfficeId === form.destinationPostOfficeId) {
      setError('Origin and destination must be different post offices.')
      return
    }

    const request = {
      type: form.type,
      weightKg: Number(form.weightKg),
      originPostOfficeId: Number(form.originPostOfficeId),
      destinationPostOfficeId: Number(form.destinationPostOfficeId),
    }

    setSaving(true)
    try {
      if (shipmentId === null) await api.createShipment(request)
      else await api.updateShipment(shipmentId, request)
      goBack()
    } catch (e) {
      setError((e as Error).message)
      setSaving(false)
    }
  }

  if (loading) return <p className="muted">Loading…</p>

  return (
    <>
      <h1>{shipmentId === null ? 'New shipment' : 'Edit shipment'}</h1>

      {shipment && (
        <p>
          <span className="number">{shipment.shipmentNumber}</span> <StatusBadge status={shipment.status} />
        </p>
      )}

      <form className="card form-stack" onSubmit={handleSubmit}>
        <label>
          Type
          <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value as ShipmentType })}>
            {shipmentTypes.map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        </label>

        <label>
          Weight (kg)
          <input
            type="number"
            min="0.001"
            step="0.001"
            value={form.weightKg}
            onChange={(e) => setForm({ ...form, weightKg: e.target.value })}
            required
          />
        </label>

        <label>
          Origin post office
          <select
            value={form.originPostOfficeId}
            onChange={(e) => setForm({ ...form, originPostOfficeId: e.target.value })}
            required
          >
            <option value="">Select…</option>
            {postOffices.map((postOffice) => (
              <option key={postOffice.id} value={postOffice.id}>
                {postOffice.zipCode} {postOffice.name}
              </option>
            ))}
          </select>
        </label>

        <label>
          Destination post office
          <select
            value={form.destinationPostOfficeId}
            onChange={(e) => setForm({ ...form, destinationPostOfficeId: e.target.value })}
            required
          >
            <option value="">Select…</option>
            {postOffices.map((postOffice) => (
              <option key={postOffice.id} value={postOffice.id}>
                {postOffice.zipCode} {postOffice.name}
              </option>
            ))}
          </select>
        </label>

        {error && <p className="error">{error}</p>}

        <div className="form-row">
          <button type="submit" className="primary" disabled={saving}>
            {shipmentId === null ? 'Create shipment' : 'Save changes'}
          </button>
          <button type="button" onClick={goBack}>
            Cancel
          </button>
        </div>
      </form>
    </>
  )
}
