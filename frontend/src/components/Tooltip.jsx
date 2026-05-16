const altDisplay = (km) => {
  if (km === 0) return "Surface";
  if (km >= 100000) return `≈ ${Math.round(km / 1000).toLocaleString()} k km`;
  return `${km.toLocaleString()} km`;
};

const fmtPrice = (p) =>
  p == null ? "—" : `$${Number(p).toFixed(2)}`;

const fmtChange = (pct) => {
  if (pct == null) return null;
  const sign = pct >= 0 ? "+" : "";
  return { text: `${sign}${Number(pct).toFixed(2)}%`, positive: pct >= 0 };
};

export function Tooltip({ company, quote, x, y }) {
  if (!company) return null;
  const change = fmtChange(quote?.changePct);
  return (
    <div className="tooltip" style={{ left: x, top: y }}>
      <div className="tt-name">{company.name}</div>
      <div className="tt-ticker mono">
        {company.exchange !== "—" ? `${company.exchange} : ${company.ticker}` : company.ticker}
      </div>
      <div className="tt-meta">
        <div>
          <div className="k">Price</div>
          <div className="v tt-price" style={{ color: quote?.price ? "var(--ink)" : undefined }}>
            {fmtPrice(quote?.price)}
            {change && (
              <span style={{ color: change.positive ? "var(--good)" : "var(--warn)", marginLeft: 6, fontSize: 10 }}>
                {change.text}
              </span>
            )}
          </div>
        </div>
        <div>
          <div className="k">Altitude</div>
          <div className="v">{altDisplay(company.altKm)}</div>
        </div>
      </div>
    </div>
  );
}
