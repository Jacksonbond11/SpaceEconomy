import { ZONES } from "../constants";

export function ZoneLines({ show, zones = ZONES }) {
  return (
    <div className="scale">
      {zones.map((z, i) => (
        <div
          key={z.id}
          className="zone-line"
          style={{
            top: `${z.yPct * 100}%`,
            opacity: show ? 1 : 0,
            transition: "opacity .3s",
            borderTopColor: i === 0 ? "transparent" : undefined,
          }}
        >
          <div className="zone-label zone-tag">
            {z.label}
            <span className="alt">{z.altRange}</span>
          </div>
        </div>
      ))}
    </div>
  );
}
