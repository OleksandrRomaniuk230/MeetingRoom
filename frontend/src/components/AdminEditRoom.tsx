import { styles } from '../styles';

interface AdminEditRoomProps {
  editRoomName: string;
  editRoomCapacity: number;
  onNameChange: (value: string) => void;
  onCapacityChange: (value: number) => void;
  onSubmit: (e: React.FormEvent) => void;
  onCancel: () => void;
}

export function AdminEditRoom({
  editRoomName,
  editRoomCapacity,
  onNameChange,
  onCapacityChange,
  onSubmit,
  onCancel,
}: AdminEditRoomProps) {
  return (
    <div className="card" style={styles.adminEditPanel}>
      <h2 style={styles.adminEditTitle}>✏️ Edit Resource Parameters</h2>
      <form onSubmit={onSubmit} style={styles.formRow}>
        <div style={styles.formFieldFull}>
          <label style={styles.formLabel}>Updated Room Name</label>
          <input
            type="text"
            value={editRoomName}
            onChange={(e) => onNameChange(e.target.value)}
            style={styles.formInput}
            required
          />
        </div>
        <div style={styles.formFieldSmall}>
          <label style={styles.formLabel}>Updated Capacity</label>
          <input
            type="number"
            value={editRoomCapacity}
            onChange={(e) => onCapacityChange(Number(e.target.value))}
            style={styles.formInput}
            min="1"
            required
          />
        </div>
        <button type="submit" style={styles.formButtonSubmit}>
          Save Changes
        </button>
        <button type="button" style={styles.formButtonCancel} onClick={onCancel}>
          Cancel
        </button>
      </form>
    </div>
  );
}
