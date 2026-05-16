import { ZONES, SECTOR_COLORS } from "../constants";
import { Icon } from "./Icons";

function hashFloat(s, salt = 0) {
  let h = 2166136261 ^ salt;
  for (let i = 0; i < s.length; i++) h = Math.imul(h ^ s.charCodeAt(i), 16777619);
  return ((h >>> 0) % 10000) / 10000;
}

export function companyY(c, zones = ZONES) {
  const z = zones.find((z) => z.id === c.zone);
  if (!z) return 50;
  const jitter = (hashFloat(c.ticker, 7) - 0.5) * 0.1;
  return (z.yPct + jitter + (c.yOffset ?? 0)) * 100;
}

export function Marker({ company, posX, posY, isDragging, selected, dimmed, showLabel, onDragStart, onClick, onHover, onLeave }) {
  const color = SECTOR_COLORS[company.sectors[0]] ?? "#7dd3fc";
  const left = posX ?? company.x * 100;
  const top = posY ?? companyY(company);
  return (
    <div
      className={`marker ${selected ? "selected" : ""} ${dimmed ? "dim" : ""} ${showLabel ? "show-label" : ""} ${isDragging ? "dragging" : ""}`}
      style={{ left: `${left}%`, top: `${top}%`, cursor: isDragging ? "grabbing" : "grab", touchAction: "none" }}
      onPointerDown={(e) => onDragStart(company, e)}
      onClick={(e) => { e.stopPropagation(); onClick(company); }}
      onMouseEnter={(e) => onHover(company, e)}
      onMouseLeave={onLeave}
      onMouseMove={(e) => onHover(company, e)}
    >
      <div className="marker-card" style={{ borderColor: selected ? undefined : `${color}55` }}>
        <div className="marker-glyph" style={{ color }}>
          <Icon id={company.icon} size={22} />
        </div>
        <div className="marker-ticker">{company.ticker}</div>
        <div className="marker-label">{company.name.split(" ")[0]}</div>
      </div>
    </div>
  );
}
