import {
  shipmentStatuses,
  shipmentTypes,
  statusLabels,
  weightCategories,
  weightCategoryLabels,
  type PostOffice,
  type ShipmentQuery,
  type ShipmentStatus,
  type ShipmentType,
  type WeightCategory,
} from '../api/types'

interface ShipmentFiltersProps {
  query: ShipmentQuery
  postOffices: PostOffice[]
  onChange: (changes: Partial<ShipmentQuery>) => void
}

export default function ShipmentFilters({ query, postOffices, onChange }: ShipmentFiltersProps) {
  // Empty select value means "no filter", so it maps to undefined.
  return (
    <div className="card form-row">
      <label>
        Shipment number
        <input
          type="search"
          placeholder="e.g. SHP-7F3K"
          value={query.shipmentNumber ?? ''}
          onChange={(e) => onChange({ shipmentNumber: e.target.value || undefined })}
        />
      </label>

      <label>
        Status
        <select
          value={query.status ?? ''}
          onChange={(e) => onChange({ status: (e.target.value || undefined) as ShipmentStatus | undefined })}
        >
          <option value="">All</option>
          {shipmentStatuses.map((status) => (
            <option key={status} value={status}>
              {statusLabels[status]}
            </option>
          ))}
        </select>
      </label>

      <label>
        Current location
        <select
          value={query.postOfficeId ?? ''}
          onChange={(e) => onChange({ postOfficeId: e.target.value ? Number(e.target.value) : undefined })}
        >
          <option value="">All</option>
          {postOffices.map((postOffice) => (
            <option key={postOffice.id} value={postOffice.id}>
              {postOffice.zipCode} {postOffice.name}
            </option>
          ))}
        </select>
      </label>

      <label>
        Weight
        <select
          value={query.weightCategory ?? ''}
          onChange={(e) =>
            onChange({ weightCategory: (e.target.value || undefined) as WeightCategory | undefined })
          }
        >
          <option value="">All</option>
          {weightCategories.map((category) => (
            <option key={category} value={category}>
              {weightCategoryLabels[category]}
            </option>
          ))}
        </select>
      </label>

      <label>
        Type
        <select
          value={query.type ?? ''}
          onChange={(e) => onChange({ type: (e.target.value || undefined) as ShipmentType | undefined })}
        >
          <option value="">All</option>
          {shipmentTypes.map((type) => (
            <option key={type} value={type}>
              {type}
            </option>
          ))}
        </select>
      </label>
    </div>
  )
}
