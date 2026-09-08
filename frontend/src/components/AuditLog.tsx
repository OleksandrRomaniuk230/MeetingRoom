import { styles } from '../styles';

interface TimeSlot {
  id: string;
  startTime: string;
  endTime: string;
  isBooked: boolean;
  bookedByUserId?: string;
  bookedByUsername?: string;
}

interface Room {
  id: string;
  name: string;
  capacity: number;
  slots?: TimeSlot[];
}

interface AuditLogProps {
  rooms: Room[];
}

// Utility function - format time from ISO string
function formatSlotLabel(isoString: string): string {
  try {
    const timePart = isoString.split('T')[1];
    return timePart.substring(0, 5);
  } catch (e) {
    return '00:00';
  }
}

export function AuditLog({ rooms }: AuditLogProps) {
  const bookedSlots = rooms.flatMap((r) =>
    (r.slots || [])
      .filter((s) => s.isBooked)
      .map((slot) => ({ room: r, slot }))
  );

  return (
    <div className="card">
      <h2 style={styles.auditTitle}>📋 Global Reservations Audit Log</h2>
      <table style={styles.table}>
        <thead>
          <tr style={styles.tableHeader}>
            <th style={styles.tableHeaderCell}>Meeting Room</th>
            <th style={styles.tableHeaderCell}>Time Interval</th>
            <th style={styles.tableHeaderCell}>Status</th>
          </tr>
        </thead>
        <tbody>
          {bookedSlots.map(({ room, slot }) => (
            <tr key={slot.id} style={styles.tableRow}>
              <td style={styles.tableCellBold}>{room.name}</td>
              <td style={styles.tableCell}>{formatSlotLabel(slot.startTime)} UTC</td>
              <td style={styles.tableCellStatus}>
                {slot.bookedByUsername || 'admin'}
              </td>
              <td style={styles.tableCell}>
                <span style={styles.tableStatusBadge}>Active Reservation</span>
              </td>
            </tr>
          ))}
          {bookedSlots.length === 0 && (
            <tr>
              <td colSpan={3} style={styles.tableEmptyRow}>
                No active bookings found across the ecosystem.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
