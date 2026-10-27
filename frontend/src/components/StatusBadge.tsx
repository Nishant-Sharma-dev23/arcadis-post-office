import { statusLabels, type ShipmentStatus } from '../api/types'

export default function StatusBadge({ status }: { status: ShipmentStatus }) {
  return <span className={`badge badge-${status}`}>{statusLabels[status]}</span>
}
