import { useState, useEffect, useMemo, useCallback, useRef } from "react";
import { ZONES, SECTORS, SECTOR_COLORS } from "./constants";
import { api } from "./api";
import { useTweaks } from "./hooks/useTweaks";
import { Marker, companyY } from "./components/Marker";
import { Tooltip } from "./components/Tooltip";
import { DetailPanel } from "./components/DetailPanel";
import { ZoneLines } from "./components/ZoneLines";

const MOBILE_BP = 768;

function useSceneSize() {
  const get = () => ({
    w: window.innerWidth,
    h: window.innerWidth <= MOBILE_BP ? window.innerHeight * 1.5 : window.innerHeight,
  });
  const [size, setSize] = useState(get);
  useEffect(() => {
    const onResize = () => setSize(get());
    window.addEventListener("resize", onResize);
    return () => window.removeEventListener("resize", onResize);
  }, []);
  return size;
}

function computeDynamicZones(companies) {
  const counts = Object.fromEntries(ZONES.map((z) => [z.id, 0]));
  companies.forEach((c) => { if (c.zone in counts) counts[c.zone]++; });
  const weights = ZONES.map((z) => Math.max(counts[z.id], 1));
  const total = weights.reduce((a, b) => a + b, 0);
  const TOP = 0.18, BOTTOM = 0.06;
  const available = 1 - TOP - BOTTOM;
  let cursor = TOP;
  return ZONES.map((z, i) => {
    const bandH = available * weights[i] / total;
    const yPct = cursor + bandH / 2;
    cursor += bandH;
    return { ...z, yPct };
  });
}

function repulsePositions(companies, w, h, zones) {
  if (!companies.length) return {};
  const MIN_PX = 68;
  const pts = companies.map((c) => ({ ticker: c.ticker, x: c.x * w, y: companyY(c, zones) / 100 * h }));
  for (let iter = 0; iter < 30; iter++) {
    for (let i = 0; i < pts.length; i++) {
      for (let j = i + 1; j < pts.length; j++) {
        const a = pts[i], b = pts[j];
        const dx = a.x - b.x, dy = a.y - b.y;
        const dist = Math.sqrt(dx * dx + dy * dy);
        if (dist < MIN_PX && dist > 0.001) {
          const push = (MIN_PX - dist) / 2;
          const nx = dx / dist, ny = dy / dist;
          a.x += nx * push; b.x -= nx * push;
          a.y += ny * push; b.y -= ny * push;
        }
      }
    }
  }
  return Object.fromEntries(pts.map((p) => [p.ticker, {
    x: Math.max(1, Math.min(99, p.x / w * 100)),
    y: Math.max(1, Math.min(99, p.y / h * 100)),
  }]));
}

const TWEAK_DEFAULTS = { labelsAlwaysOn: false, showZoneLines: true };
const QUOTE_POLL_MS = 5 * 60 * 1000;
const LS_KEY = "se-node-positions";

export function App() {
  const [t, setTweak] = useTweaks(TWEAK_DEFAULTS);
  const { w, h } = useSceneSize();
  const [companies, setCompanies] = useState([]);
  const [quotes, setQuotes] = useState({});
  const [selected, setSelected] = useState(null);
  const [hover, setHover] = useState(null);
  const [mousePos, setMousePos] = useState({ x: 0, y: 0 });
  const [activeSectors, setActiveSectors] = useState(new Set());
  const [search, setSearch] = useState("");
  const [showResults, setShowResults] = useState(false);

  const sceneRef = useRef(null);
  const draggingRef = useRef(null);
  const suppressClickRef = useRef(false);
  const [activeDrag, setActiveDrag] = useState(null);
  const [dragPositions, setDragPositions] = useState(() => {
    try { return JSON.parse(localStorage.getItem(LS_KEY) ?? "{}"); }
    catch { return {}; }
  });

  useEffect(() => {
    api.companies().then((data) => {
      setCompanies(data);
      const q = {};
      data.forEach((c) => { if (c.quote) q[c.ticker] = c.quote; });
      setQuotes(q);
    });
  }, []);

  useEffect(() => {
    const poll = () => api.quotes().then(setQuotes).catch(() => {});
    const id = setInterval(poll, QUOTE_POLL_MS);
    return () => clearInterval(id);
  }, []);

  useEffect(() => {
    const onKey = (e) => {
      if (e.key === "Escape") { setSelected(null); setShowResults(false); }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, []);

  const visibleSet = useMemo(() => new Set(
    companies
      .filter((c) => activeSectors.size === 0 || c.sectors.some((s) => activeSectors.has(s)))
      .filter((c) => !search.trim() || c.ticker.toLowerCase().includes(search.toLowerCase()) || c.name.toLowerCase().includes(search.toLowerCase()))
      .map((c) => c.ticker)
  ), [companies, activeSectors, search]);

  const searchMatches = useMemo(() => {
    if (!search.trim()) return [];
    const q = search.toLowerCase();
    return companies.filter((c) => c.ticker.toLowerCase().includes(q) || c.name.toLowerCase().includes(q)).slice(0, 6);
  }, [companies, search]);

  const onMarkerHover = useCallback((c, e) => {
    setHover(c);
    setMousePos({ x: e.clientX, y: e.clientY });
  }, []);

  const toggleSector = useCallback((s) => {
    setActiveSectors((prev) => {
      const next = new Set(prev);
      if (next.has(s)) next.delete(s); else next.add(s);
      return next;
    });
  }, []);

  const dynamicZones = useMemo(() => computeDynamicZones(companies), [companies]);
  const resolvedPositions = useMemo(() => repulsePositions(companies, w, h, dynamicZones), [companies, w, h, dynamicZones]);

  const finalPositions = useMemo(() => Object.fromEntries(
    companies.map((c) => [c.ticker, dragPositions[c.ticker] ?? resolvedPositions[c.ticker] ?? { x: c.x * 100, y: companyY(c, dynamicZones) }])
  ), [companies, resolvedPositions, dragPositions, dynamicZones]);

  const startDrag = useCallback((company, e) => {
    e.preventDefault();
    const scene = sceneRef.current.getBoundingClientRect();
    const pos = finalPositions[company.ticker];
    draggingRef.current = {
      ticker: company.ticker,
      origX: pos.x,
      origY: pos.y,
      startClientX: e.clientX,
      startClientY: e.clientY,
      sceneW: scene.width,
      sceneH: scene.height,
      moved: false,
    };
    setActiveDrag(company.ticker);
  }, [finalPositions]);

  const onScenePointerMove = useCallback((e) => {
    if (!draggingRef.current) return;
    const d = draggingRef.current;
    const dx = e.clientX - d.startClientX;
    const dy = e.clientY - d.startClientY;
    if (Math.sqrt(dx * dx + dy * dy) > 4) d.moved = true;
    if (!d.moved) return;
    const newX = Math.max(1, Math.min(99, d.origX + dx / d.sceneW * 100));
    const newY = Math.max(1, Math.min(99, d.origY + dy / d.sceneH * 100));
    setDragPositions((prev) => ({ ...prev, [d.ticker]: { x: newX, y: newY } }));
  }, []);

  const onScenePointerUp = useCallback(() => {
    if (!draggingRef.current) return;
    const d = draggingRef.current;
    if (d.moved) {
      suppressClickRef.current = true;
      setDragPositions((prev) => {
        localStorage.setItem(LS_KEY, JSON.stringify(prev));
        return prev;
      });
    }
    draggingRef.current = null;
    setActiveDrag(null);
  }, []);

  const onSceneClick = useCallback(() => {
    if (suppressClickRef.current) { suppressClickRef.current = false; return; }
    setSelected(null);
  }, []);

  const hoverQuote = hover ? quotes[hover.ticker] : null;
  const selectedQuote = selected ? quotes[selected.ticker] : null;

  return (
    <>
      <div
        className="scene"
        ref={sceneRef}
        onClick={onSceneClick}
        onPointerMove={onScenePointerMove}
        onPointerUp={onScenePointerUp}
      >
        <ZoneLines show={t.showZoneLines} zones={dynamicZones} />
        {companies.map((c) => {
          const pos = finalPositions[c.ticker];
          return (
            <Marker
              key={c.ticker}
              company={c}
              posX={pos?.x}
              posY={pos?.y}
              isDragging={activeDrag === c.ticker}
              selected={selected?.ticker === c.ticker}
              dimmed={!visibleSet.has(c.ticker)}
              showLabel={t.labelsAlwaysOn}
              onDragStart={startDrag}
              onClick={(c) => {
                if (suppressClickRef.current) { suppressClickRef.current = false; return; }
                setSelected(c);
                setHover(null);
              }}
              onHover={onMarkerHover}
              onLeave={() => setHover(null)}
            />
          );
        })}
        <Tooltip company={hover} quote={hoverQuote} x={mousePos.x} y={mousePos.y} />
      </div>

      <div className="topbar">
        <div className="topbar-row">
          <div className="brand">
            <div className="brand-mark" />
            <div>
              <div className="brand-title">Space Economy</div>
              <div className="brand-sub mono">Space Markets · Orbital Map</div>
            </div>
          </div>

          <div className="search">
            <svg className="search-icon" width="14" height="14" viewBox="0 0 14 14" fill="none" stroke="currentColor" strokeWidth="1.5">
              <circle cx="6" cy="6" r="4.5" />
              <path d="M9.5 9.5 L13 13" />
            </svg>
            <input
              type="text"
              placeholder="Search ticker or company…"
              value={search}
              onChange={(e) => { setSearch(e.target.value); setShowResults(true); }}
              onFocus={() => setShowResults(true)}
              onBlur={() => setTimeout(() => setShowResults(false), 150)}
            />
            {showResults && searchMatches.length > 0 && (
              <div className="search-results">
                {searchMatches.map((c) => (
                  <div key={c.ticker} className="res" onMouseDown={() => { setSelected(c); setSearch(""); setShowResults(false); }}>
                    <div>
                      <div className="t-name">{c.name}</div>
                      <div className="t-ticker mono">{c.exchange !== "—" ? `${c.exchange} : ${c.ticker}` : c.ticker}</div>
                    </div>
                    <div className="mono" style={{ fontSize: 10, color: "var(--ink-faint)", letterSpacing: ".12em", textTransform: "uppercase" }}>
                      {c.zone}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          <div className="topbar-meta" style={{ marginLeft: "auto", display: "flex", alignItems: "center", gap: 14 }}>
            {Object.keys(dragPositions).length > 0 && (
              <button
                onClick={() => { setDragPositions({}); localStorage.removeItem(LS_KEY); }}
                style={{ background: "none", border: "none", cursor: "pointer", fontFamily: "'JetBrains Mono', monospace", fontSize: 10, color: "var(--ink-faint)", letterSpacing: ".12em", textTransform: "uppercase", padding: 0 }}
              >
                Reset layout
              </button>
            )}
            <div style={{ fontFamily: "'JetBrains Mono', monospace", fontSize: 11, color: "var(--ink-dim)", letterSpacing: ".14em", textTransform: "uppercase" }}>
              <span style={{ color: "var(--ink)" }}>{visibleSet.size}</span>
              <span style={{ color: "var(--ink-faint)" }}> / {companies.length} </span>
              <span style={{ color: "var(--ink-faint)", fontSize: 10 }}>tracked</span>
            </div>
          </div>
        </div>

        <div className="topbar-row">
          <div className="chips-label mono">Filter</div>
          <div className="chips">
            <div className={`chip chip-all ${activeSectors.size === 0 ? "active" : ""}`} onClick={() => setActiveSectors(new Set())}>
              All <span className="count mono">{companies.length}</span>
            </div>
            {SECTORS.map((s) => {
              const n = companies.filter((c) => c.sectors.includes(s)).length;
              return (
                <div
                  key={s}
                  className={`chip ${activeSectors.has(s) ? "active" : ""}`}
                  onClick={() => toggleSector(s)}
                  style={activeSectors.has(s) ? { borderColor: `${SECTOR_COLORS[s]}80`, background: `${SECTOR_COLORS[s]}22`, color: "var(--ink)" } : {}}
                >
                  <span style={{ width: 6, height: 6, borderRadius: "50%", background: SECTOR_COLORS[s], display: "inline-block" }} />
                  {s} <span className="count mono">{n}</span>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      <DetailPanel company={selected} quote={selectedQuote} onClose={() => setSelected(null)} />
    </>
  );
}

export default App;
