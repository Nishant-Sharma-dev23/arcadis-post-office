import type {
  PagedResult,
  PostOffice,
  PostOfficeRequest,
  Shipment,
  ShipmentQuery,
  ShipmentRequest,
} from './types'

const baseUrl = import.meta.env.VITE_API_URL ?? 'http://localhost:5167'

/** Shape of an ASP.NET Core ProblemDetails error response. */
interface ProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

/** Error thrown for any non-2xx response, with a message that is safe to show to the user. */
export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${baseUrl}/api${path}`, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })

  if (!response.ok) {
    throw new ApiError(response.status, await readErrorMessage(response))
  }

  // 204 No Content has no body.
  return (response.status === 204 ? undefined : await response.json()) as T
}

async function readErrorMessage(response: Response): Promise<string> {
  try {
    const problem = (await response.json()) as ProblemDetails
    if (problem.errors) return Object.values(problem.errors).flat().join(' ')
    return problem.detail ?? problem.title ?? `Request failed (${response.status})`
  } catch {
    return `Request failed (${response.status})`
  }
}

function toQueryString(query: ShipmentQuery): string {
  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== '') params.set(key, String(value))
  }
  return params.toString()
}

export const api = {
  getPostOffices: () => request<PostOffice[]>('/post-offices'),
  createPostOffice: (body: PostOfficeRequest) =>
    request<PostOffice>('/post-offices', { method: 'POST', body: JSON.stringify(body) }),
  updatePostOffice: (id: number, body: PostOfficeRequest) =>
    request<PostOffice>(`/post-offices/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  deletePostOffice: (id: number) => request<void>(`/post-offices/${id}`, { method: 'DELETE' }),

  getShipments: (query: ShipmentQuery) =>
    request<PagedResult<Shipment>>(`/shipments?${toQueryString(query)}`),
  getShipment: (id: number) => request<Shipment>(`/shipments/${id}`),
  createShipment: (body: ShipmentRequest) =>
    request<Shipment>('/shipments', { method: 'POST', body: JSON.stringify(body) }),
  updateShipment: (id: number, body: ShipmentRequest) =>
    request<Shipment>(`/shipments/${id}`, { method: 'PUT', body: JSON.stringify(body) }),
  advanceShipmentStatus: (id: number) =>
    request<Shipment>(`/shipments/${id}/advance-status`, { method: 'POST' }),
  deleteShipment: (id: number) => request<void>(`/shipments/${id}`, { method: 'DELETE' }),
}
