import { styles } from '../styles';

interface HeaderProps {
  userRole: string | null;
  isAdmin: boolean;
  showAllBookings: boolean;
  onToggleAudit: () => void;
  onLogout: () => void;
}

export function Header({
  userRole,
  isAdmin,
  showAllBookings,
  onToggleAudit,
  onLogout,
}: HeaderProps) {
  return (
    <div style={styles.headerContainer}>
      <div>
        <h1 style={styles.headerTitle}>Room Booking Dashboard</h1>
        <p style={styles.headerSubtitle}>
          Logged in as: <strong style={styles.headerRole}>{userRole}</strong> (Real-time Active 🟢)
        </p>
      </div>
      <div style={styles.headerButtonRow}>
        {isAdmin && (
          <button
            onClick={onToggleAudit}
            style={{ backgroundColor: '#4b5563' }}
          >
            {showAllBookings ? 'View Schedule Grid' : '🔍 View All Bookings (Audit)'}
          </button>
        )}
        <button onClick={onLogout} style={{ backgroundColor: '#ef4444' }}>
          Sign Out
        </button>
      </div>
    </div>
  );
}
