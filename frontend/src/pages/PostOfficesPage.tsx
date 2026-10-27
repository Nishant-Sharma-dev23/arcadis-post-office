import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../api/client'
import type { PostOffice } from '../api/types'

const emptyForm = { zipCode: '', name: '' }

export default function PostOfficesPage() {
  const [postOffices, setPostOffices] = useState<PostOffice[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // The form is used for both add and edit; editingId is null when adding.
  const [form, setForm] = useState(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [saving, setSaving] = useState(false)

  const [reloadKey, setReloadKey] = useState(0) // bump to reload after a change

  useEffect(() => {
    api
      .getPostOffices()
      .then(
        (data) => {
          setPostOffices(data)
          setError(null)
        },
        (e: Error) => setError(e.message),
      )
      .finally(() => setLoading(false))
  }, [reloadKey])

  const reload = () => setReloadKey((key) => key + 1)

  function startEdit(postOffice: PostOffice) {
    setEditingId(postOffice.id)
    setForm({ zipCode: postOffice.zipCode, name: postOffice.name })
    setError(null)
  }

  function resetForm() {
    setEditingId(null)
    setForm(emptyForm)
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    setSaving(true)
    try {
      if (editingId === null) await api.createPostOffice(form)
      else await api.updatePostOffice(editingId, form)
      resetForm()
      reload()
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(postOffice: PostOffice) {
    if (!confirm(`Delete post office ${postOffice.zipCode} ${postOffice.name}?`)) return
    try {
      await api.deletePostOffice(postOffice.id)
      if (editingId === postOffice.id) resetForm()
      reload()
    } catch (e) {
      // e.g. 409 when shipments still use this post office
      setError((e as Error).message)
    }
  }

  return (
    <>
      <h1>Post offices</h1>

      <form className="card form-row" onSubmit={handleSubmit}>
        <label>
          ZIP code
          <input
            value={form.zipCode}
            onChange={(e) => setForm({ ...form, zipCode: e.target.value })}
            required
            pattern="\d{5}"
            title="Exactly 5 digits"
            maxLength={5}
          />
        </label>
        <label>
          Name
          <input
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            required
            maxLength={100}
          />
        </label>
        <button type="submit" className="primary" disabled={saving}>
          {editingId === null ? 'Add post office' : 'Save changes'}
        </button>
        {editingId !== null && (
          <button type="button" onClick={resetForm}>
            Cancel
          </button>
        )}
      </form>

      {error && <p className="error">{error}</p>}

      {loading ? (
        <p className="muted">Loading…</p>
      ) : postOffices.length === 0 ? (
        <p className="muted">No post offices yet.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ZIP code</th>
              <th>Name</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {postOffices.map((postOffice) => (
              <tr key={postOffice.id}>
                <td>{postOffice.zipCode}</td>
                <td>{postOffice.name}</td>
                <td className="actions">
                  <button onClick={() => startEdit(postOffice)}>Edit</button>
                  <button className="danger" onClick={() => handleDelete(postOffice)}>
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  )
}
