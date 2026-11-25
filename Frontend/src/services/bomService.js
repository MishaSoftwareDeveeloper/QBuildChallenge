const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080').replace(/\/$/, '');

async function readJson(response) {
  const text = await response.text();
  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text);
  } catch {
    throw new Error('The API returned a non-JSON response.');
  }
}

async function request(path) {
  let response;
  try {
    response = await fetch(`${API_BASE_URL}${path}`);
  } catch {
    throw new Error('Unable to reach the BOM API. Confirm the backend is running.');
  }

  const payload = await readJson(response);
  if (!response.ok) {
    const message = payload?.message || `Request failed with status ${response.status}.`;
    throw new Error(message);
  }

  return payload;
}

export function getBomTree() {
  return request('/api/bom/tree');
}

export function getComponents(parentName) {
  return request(`/api/bom/components/${encodeURIComponent(parentName)}`);
}
