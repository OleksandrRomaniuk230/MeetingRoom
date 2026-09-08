import { styles } from '../styles';

interface AdminCreateRoomProps {
  newRoomName: string;
  newRoomCapacity: number;
  onNameChange: (value: string) => void;
  onCapacityChange: (value: number) => void;
  onSubmit: (e: React.FormEvent) => void;
}

export function AdminCreateRoom({
  newRoomName,
  newRoomCapacity,
  onNameChange,
  onCapacityChange,
  onSubmit,
}: AdminCreateRoomProps) {
  return (
    <div className="card" style={styles.adminCreatePanel}>
      <h2 style={styles.adminCreateTitle}>🛡️ Admin Management Panel (Create Resource)</h2>
      <form onSubmit={onSubmit} style={styles.formRow}>
        <div style={styles.formFieldFull}>
          <label style={styles.formLabel}>Room Name</label>
          <input
            type="text"
            value={newRoomName}
            onChange={(e) => onNameChange(e.target.value)}
            style={styles.formInput}
            required
          />
        </div>
        <div style={styles.formFieldSmall}>
          <label style={styles.formLabel}>Capacity</label>
          <input
            type="number"
            value={newRoomCapacity}
            onChange={(e) => onCapacityChange(Number(e.target.value))}
            style={styles.formInput}
            min="1"
            required
          />
        </div>
        <button type="submit" style={styles.formButton}>
          Create New Room
        </button>
      </form>
    </div>
  );
}
