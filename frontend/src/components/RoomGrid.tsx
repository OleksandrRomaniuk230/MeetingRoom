import { styles } from '../styles';
import { SlotButton } from './SlotButton';

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

interface RoomGridProps {
  rooms: Room[];
  isAdmin: boolean;
  currentUserId: string | null;
  token: string | null;
  loading: boolean;
  onEditRoom: (roomId: string, roomName: string, capacity: number) => void;
  onDeleteRoom: (roomId: string) => void;
  onMessage: (msg: string) => void;
  onRefresh: () => Promise<void>;
}

export function RoomGrid({
  rooms,
  isAdmin,
  currentUserId,
  token,
  loading,
  onEditRoom,
  onDeleteRoom,
  onMessage,
  onRefresh,
}: RoomGridProps) {
  return (
    <div>
      {loading && <p>Updating slot matrices...</p>}
      <div className="grid">
        {rooms.map((room) => (
          <div key={room.id} className="card" style={styles.roomCardContainer}>
            {isAdmin && (
              <div style={styles.adminButtonRow}>
                <button
                  onClick={() => onEditRoom(room.id, room.name, room.capacity)}
                  style={styles.adminEditButton}
                >
                  Edit
                </button>
                <button
                  onClick={() => onDeleteRoom(room.id)}
                  style={styles.adminDeleteButton}
                >
                  Delete
                </button>
              </div>
            )}

            <h3 style={styles.roomTitle}>{room.name}</h3>
            <span style={styles.roomCapacity}>Capacity: {room.capacity} people</span>

            <div style={styles.slotGrid}>
              {room.slots?.map((slot) => (
                <SlotButton
                  key={slot.id}
                  slot={slot}
                  currentUserId={currentUserId}
                  isAdmin={isAdmin}
                  token={token}
                  onMessage={onMessage}
                  onRefresh={onRefresh}
                />
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
