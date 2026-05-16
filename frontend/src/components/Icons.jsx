export function Icon({ id, size = 22 }) {
  const s = { width: size, height: size, display: "block" };
  const common = { fill: "none", stroke: "currentColor", strokeWidth: "1.5", strokeLinecap: "round", strokeLinejoin: "round" };
  switch (id) {
    case "rocket":
      return (
        <svg viewBox="0 0 24 24" style={s} {...common}>
          <path d="M12 3 L16 14 L12 12 L8 14 Z" />
          <path d="M10 18 L12 15 L14 18" />
          <line x1="12" y1="20" x2="12" y2="22" opacity="0.5" />
        </svg>
      );
    case "platform":
      return (
        <svg viewBox="0 0 24 24" style={s} {...common}>
          <rect x="9" y="9" width="6" height="6" rx="1" />
          <rect x="1" y="10" width="7" height="4" />
          <rect x="16" y="10" width="7" height="4" />
          <line x1="3" y1="10" x2="3" y2="14" opacity="0.6"/>
          <line x1="5" y1="10" x2="5" y2="14" opacity="0.6"/>
          <line x1="19" y1="10" x2="19" y2="14" opacity="0.6"/>
          <line x1="21" y1="10" x2="21" y2="14" opacity="0.6"/>
        </svg>
      );
    case "imager":
      return (
        <svg viewBox="0 0 24 24" style={s} {...common}>
          <rect x="8" y="4" width="8" height="6" rx="1" />
          <rect x="2" y="5" width="5" height="4" />
          <rect x="17" y="5" width="5" height="4" />
          <circle cx="12" cy="16" r="4" />
          <circle cx="12" cy="16" r="1.5" fill="currentColor"/>
        </svg>
      );
    case "cubesat":
      return (
        <svg viewBox="0 0 24 24" style={s} {...common}>
          <rect x="7" y="9" width="10" height="10" rx="1" />
          <line x1="7" y1="14" x2="17" y2="14" opacity="0.5"/>
          <line x1="12" y1="9" x2="12" y2="19" opacity="0.5"/>
          <path d="M9 9 L7 4" />
          <path d="M15 9 L17 4" />
        </svg>
      );
    case "sat-big":
      return (
        <svg viewBox="0 0 24 24" style={s} {...common}>
          <rect x="6" y="6" width="12" height="9" />
          <line x1="9" y1="6" x2="9" y2="15" opacity="0.5"/>
          <line x1="12" y1="6" x2="12" y2="15" opacity="0.5"/>
          <line x1="15" y1="6" x2="15" y2="15" opacity="0.5"/>
          <line x1="6" y1="9" x2="18" y2="9" opacity="0.5"/>
          <line x1="6" y1="12" x2="18" y2="12" opacity="0.5"/>
          <line x1="12" y1="15" x2="12" y2="20" />
          <line x1="9" y1="20" x2="15" y2="20" />
        </svg>
      );
    case "constellation":
      return (
        <svg viewBox="0 0 24 24" style={s} fill="none" stroke="currentColor" strokeWidth="1.5">
          <ellipse cx="12" cy="12" rx="9" ry="4" />
          <circle cx="3" cy="12" r="1.5" fill="currentColor" stroke="none"/>
          <circle cx="15" cy="9" r="1.5" fill="currentColor" stroke="none"/>
          <circle cx="18" cy="14" r="1.5" fill="currentColor" stroke="none"/>
        </svg>
      );
    case "geo-sat":
      return (
        <svg viewBox="0 0 24 24" style={s} {...common}>
          <rect x="9" y="3" width="6" height="6" rx="1"/>
          <rect x="3" y="4" width="5" height="4"/>
          <rect x="16" y="4" width="5" height="4"/>
          <path d="M12 9 L9 15 L15 15 Z" />
          <path d="M7 19 Q12 22 17 19" opacity="0.6"/>
        </svg>
      );
    case "lander":
      return (
        <svg viewBox="0 0 24 24" style={s} {...common}>
          <path d="M8 6 L16 6 L14 13 L10 13 Z"/>
          <line x1="10" y1="13" x2="6" y2="20"/>
          <line x1="14" y1="13" x2="18" y2="20"/>
          <line x1="12" y1="9" x2="12" y2="20" opacity="0.5"/>
          <line x1="5" y1="20" x2="7" y2="20"/>
          <line x1="17" y1="20" x2="19" y2="20"/>
        </svg>
      );
    default:
      return (
        <svg viewBox="0 0 24 24" style={s} fill="none" stroke="currentColor" strokeWidth="1.5">
          <circle cx="12" cy="12" r="6"/>
        </svg>
      );
  }
}
