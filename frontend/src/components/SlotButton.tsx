import { styles } from '../styles';

interface TimeSlot {
  id: string;
  startTime: string;
  endTime: string;
  isBooked: boolean;
  bookedByUserId?: string;
  bookedByUsername?: string;
}

interface SlotButtonProps {
  slot: TimeSlot;
  currentUserId: string | null;
  isAdmin: boolean;
  token: string | null;
  onMessage: (msg: string) => void;
  onRefresh: () => Promise<void>;
}

const API_URL = 'http://localhost:5080/api';

// Utility function - format time from ISO string
function formatSlotLabel(isoString: string): string {
  try {
    const timePart = isoString.split('T')[1];
    return timePart.substring(0, 5);
  } catch (e) {
    return '00:00';
  }
}

export function SlotButton({
  slot,
  currentUserId,
  isAdmin,
  token,
  onMessage,
  onRefresh,
}: SlotButtonProps) {
  // Handle booking a slot
  const handleBookSlot = async (slotId: string) => {
    onMessage('');
    try {
      const res = await fetch(`${API_URL}/bookings`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({ timeSlotId: slotId }),
      });

      if (res.status === 409) {
        throw new Error(
          'Conflict! This slot has just been reserved by another user.'
        );
      }
      if (!res.ok)
        throw new Error('An error occurred while processing the reservation.');

      onMessage('Room successfully booked!');
      setTimeout(() => onMessage(''), 5000);
      await onRefresh();
    } catch (err: any) {
      onMessage(err.message);
      setTimeout(() => onMessage(''), 5000);
    }
  };

  // Handle canceling a slot
  const handleCancelSlot = async (slotId: string) => {
    if (!window.confirm('Do you want to cancel this booking?')) return;
    onMessage('');
    try {
      const res = await fetch(`${API_URL}/bookings/${slotId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` },
      });

      if (res.status === 403) {
        throw new Error(
          'Forbidden! You can only cancel your own reservations.'
        );
      }
      if (!res.ok) throw new Error('Failed to cancel the booking.');

      onMessage('Booking successfully canceled.');
      setTimeout(() => onMessage(''), 5000);
      await onRefresh();
    } catch (err: any) {
      onMessage(err.message);
      setTimeout(() => onMessage(''), 5000);
    }
  };

  // CRITICAL: Preserve exact 07:00-18:00 color matrix logic
  const slotUserId = slot.bookedByUserId
    ? String(slot.bookedByUserId).toLowerCase()
    : '';
  const authedUserId = currentUserId
    ? String(currentUserId).toLowerCase()
    : '';
  const isOwnBooking = slotUserId !== '' && slotUserId === authedUserId;
  const isRedState = isAdmin || isOwnBooking;

  let buttonColor: string = styles.slotColorFree; // green - available
  let cursorStyle = 'pointer';

  if (slot.isBooked) {
    if (isRedState) {
      buttonColor = styles.slotColorOwn; // red - can cancel
    } else {
      buttonColor = styles.slotColorOther; // gray - cannot interact
      cursorStyle = 'not-allowed';
    }
  }

  const canClick = !slot.isBooked || isRedState;

  const handleClick = () => {
    if (!canClick) return;
    if (slot.isBooked) {
      handleCancelSlot(slot.id);
    } else {
      handleBookSlot(slot.id);
    }
  };

  return (
    <button
      className={`slot-btn ${slot.isBooked ? 'slot-booked' : 'slot-free'}`}
      style={{
        ...styles.slotButton,
        backgroundColor: buttonColor,
        cursor: cursorStyle,
      }}
      onClick={handleClick}
    >
      {formatSlotLabel(slot.startTime)}
    </button>
  );
}
