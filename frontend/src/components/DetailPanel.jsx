import { useState, useEffect } from "react";
import { ZONES, SECTOR_COLORS } from "../constants";
import { api } from "../api";

const altDisplay = (km) => {
  if (km === 0) return "Surface";
  if (km >= 100000) return `≈ ${Math.round(km / 1000).toLocaleString()} k km`;
  return `${km.toLocaleString()} km`;
};

const fmtPrice = (p) => (p == null ? "—" : `$${Number(p).toFixed(2)}`);
const fmtMarketCap = (mc) => {
  if (mc == null) return "—";
  const n = Number(mc);
  if (n >= 1e9) return `$${(n / 1e9).toFixed(2)}B`;
  if (n >= 1e6) return `$${(n / 1e6).toFixed(1)}M`;
  return `$${n.toLocaleString()}`;
};
const fmtChange = (pct) => {
  if (pct == null) return null;
  const sign = pct >= 0 ? "+" : "";
  return { text: `${sign}${Number(pct).toFixed(2)}%`, positive: pct >= 0 };
};
const zoneLabelOf = (zoneId) => ZONES.find((z) => z.id === zoneId)?.label ?? zoneId;
const relativeTime = (iso) => {
  const ms = Date.now() - new Date(iso).getTime();
  const h = ms / 3.6e6;
  if (h < 1) return `${Math.round(h * 60)}m ago`;
  if (h < 24) return `${Math.round(h)}h ago`;
  return `${Math.round(h / 24)}d ago`;
};

export function DetailPanel({ company, quote, onClose }) {
  const [shown, setShown] = useState(null);
  const [news, setNews] = useState([]);
  const [newsLoading, setNewsLoading] = useState(false);

  useEffect(() => {
    if (company) {
      setShown(company);
      setNews([]);
      setNewsLoading(true);
      api.company(company.ticker)
        .then((data) => setNews(data.news ?? []))
        .catch(() => {})
        .finally(() => setNewsLoading(false));
    } else {
      const t = setTimeout(() => setShown(null), 650);
      return () => clearTimeout(t);
    }
  }, [company]);

  const c = shown;
  const open = !!company;
  const maxKm = 400000;
  const pct = !c || c.altKm <= 0
    ? 0
    : Math.max(2, Math.min(100, (Math.log10(Math.max(1, c.altKm)) / Math.log10(maxKm)) * 100));

  const change = fmtChange(quote?.changePct);

  return (
    <aside className={`panel ${open ? "open" : ""}`} aria-hidden={!open}>
      {c && (
        <>
          <button className="panel-close" onClick={onClose} aria-label="Close">
            <svg width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" strokeWidth="1.5">
              <path d="M2 2 L12 12 M12 2 L2 12" />
            </svg>
          </button>

          <div className="panel-eyebrow mono">{zoneLabelOf(c.zone)}</div>
          <h2>{c.name}</h2>
          <div className="ticker-row">
            <div className="panel-ticker mono">{c.ticker}</div>
            <div className="exch mono">{c.exchange}</div>
          </div>

          <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
            <div className="orbit-readout">
              <div className="orbit-bar" style={{ "--p": `${pct}%` }} />
              <div className="orbit-alt mono">{altDisplay(c.altKm)}</div>
            </div>
            <div className="mono" style={{ fontSize: 10, color: "var(--ink-faint)", letterSpacing: ".14em", textTransform: "uppercase" }}>
              Representative operating altitude
            </div>
          </div>

          <div className="panel-section">
            <h3>Description</h3>
            <p className="panel-desc">{c.desc}</p>
          </div>

          <div className="panel-section">
            <h3>Sectors</h3>
            <div className="tags">
              {c.sectors.map((s) => (
                <span key={s} className="tag" style={{ color: SECTOR_COLORS[s], borderColor: `${SECTOR_COLORS[s]}50`, background: `${SECTOR_COLORS[s]}15` }}>
                  {s}
                </span>
              ))}
            </div>
          </div>

          <div className="panel-section">
            <h3>Snapshot</h3>
            <div className="stat-grid">
              <div className="stat">
                <div className="k">Last price</div>
                <div className="v" style={{ color: quote?.price ? (change?.positive ? "var(--good)" : "var(--warn)") : "var(--ink-faint)" }}>
                  {fmtPrice(quote?.price)}
                  {change && <span style={{ fontSize: 11, marginLeft: 6, color: change.positive ? "var(--good)" : "var(--warn)" }}>{change.text}</span>}
                </div>
              </div>
              <div className="stat">
                <div className="k">Market cap</div>
                <div className="v">{fmtMarketCap(quote?.marketCap)}</div>
              </div>
              <div className="stat">
                <div className="k">Day range</div>
                <div className="v" style={{ fontSize: 11 }}>
                  {quote?.low && quote?.high ? `${fmtPrice(quote.low)} – ${fmtPrice(quote.high)}` : "—"}
                </div>
              </div>
              <div className="stat">
                <div className="k">Volume</div>
                <div className="v" style={{ fontSize: 11 }}>
                  {quote?.volume ? Number(quote.volume).toLocaleString() : "—"}
                </div>
              </div>
              <div className="stat">
                <div className="k">Founded</div>
                <div className="v">{c.founded ?? "—"}</div>
              </div>
              <div className="stat">
                <div className="k">HQ</div>
                <div className="v" style={{ fontSize: 12 }}>{c.hq ?? "—"}</div>
              </div>
            </div>
          </div>

          <div className="panel-section">
            <h3>News</h3>
            {newsLoading ? (
              <div className="news-loading mono">Loading…</div>
            ) : news.length === 0 ? (
              <div className="news-empty mono">No recent news</div>
            ) : (
              <div className="news-list">
                {news.map((article) => (
                  <a
                    key={article.id}
                    href={article.articleUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="news-item"
                  >
                    {article.imageUrl && (
                      <img className="news-thumb" src={article.imageUrl} alt="" loading="lazy" />
                    )}
                    <div className="news-body">
                      <div className="news-title">{article.title}</div>
                      <div className="news-meta">
                        <span>{article.publisher}</span>
                        <span className="news-dot">·</span>
                        <span>{relativeTime(article.publishedUtc)}</span>
                      </div>
                    </div>
                  </a>
                ))}
              </div>
            )}
          </div>
        </>
      )}
    </aside>
  );
}
