// Mirrors the backend DTOs in Postal.Api/Contracts. Enums are sent as strings.

export const shipmentTypes = ['Letter', 'Package'] as const
export type ShipmentType = (typeof shipmentTypes)[number]

export const shipmentStatuses = ['ReceivedAtOrigin', 'ReceivedAtDestination', 'Delivered'] as const
export type ShipmentStatus = (typeof shipmentStatuses)[number]

export const weightCategories = ['UnderOneKg', 'OneToFiveKg', 'OverFiveKg'] as const
export type WeightCategory = (typeof weightCategories)[number]

export const statusLabels: Record<ShipmentStatus, string> = {
  ReceivedAtOrigin: 'Received at origin',
  ReceivedAtDestination: 'Received at destination',
  Delivered: 'Delivered',
}

export const weightCategoryLabels: Record<WeightCategory, string> = {
  UnderOneKg: 'Under 1 kg',
  OneToFiveKg: '1–5 kg',
  OverFiveKg: 'Over 5 kg',
}

export interface PostOffice {
  id: number
  zipCode: string
  name: string
}

export interface PostOfficeRequest {
  zipCode: string
  name: string
}

export interface Shipment {
  id: number
  shipmentNumber: string
  type: ShipmentType
  weightKg: number
  weightCategory: WeightCategory
  status: ShipmentStatus
  origin: PostOffice
  destination: PostOffice
  currentPostOffice: PostOffice
  createdAt: string
  updatedAt: string
}

export interface ShipmentRequest {
  type: ShipmentType
  weightKg: number
  originPostOfficeId: number
  destinationPostOfficeId: number
}

export interface ShipmentQuery {
  page: number
  pageSize: number
  shipmentNumber?: string
  status?: ShipmentStatus
  postOfficeId?: number
  weightCategory?: WeightCategory
  type?: ShipmentType
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}
