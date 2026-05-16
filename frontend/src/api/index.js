const BASE = import.meta.env.VITE_API_URL ?? "";

async function get(path) {
  const res = await fetch(`${BASE}${path}`);
  if (!res.ok) throw new Error(`${res.status} ${path}`);
  return res.json();
}

export const api = {
  companies: () => get("/api/companies"),
  company: (ticker) => get(`/api/companies/${ticker}`),
  quotes: () => get("/api/quotes"),
  news: (ticker) => get(`/api/news?ticker=${ticker}&limit=8`),
  latestNews: () => get("/api/news?limit=20"),
};
