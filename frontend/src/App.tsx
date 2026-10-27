import { NavLink, Navigate, Route, Routes } from 'react-router'
import PostOfficesPage from './pages/PostOfficesPage'
import ShipmentFormPage from './pages/ShipmentFormPage'
import ShipmentsPage from './pages/ShipmentsPage'

export default function App() {
  return (
    <>
      <header className="top-bar">
        <span className="brand">Post Office Shipments</span>
        <nav>
          <NavLink to="/shipments" end>Shipments</NavLink>
          <NavLink to="/post-offices">Post offices</NavLink>
        </nav>
      </header>

      <main className="container">
        <Routes>
          <Route path="/" element={<Navigate to="/shipments" replace />} />
          <Route path="/shipments" element={<ShipmentsPage />} />
          <Route path="/shipments/new" element={<ShipmentFormPage />} />
          <Route path="/shipments/:id/edit" element={<ShipmentFormPage />} />
          <Route path="/post-offices" element={<PostOfficesPage />} />
          <Route path="*" element={<p>Page not found.</p>} />
        </Routes>
      </main>
    </>
  )
}
