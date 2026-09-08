import { useState, useImperativeHandle, forwardRef } from 'react';
import { AdminCreateRoom } from './AdminCreateRoom';
import { AdminEditRoom } from './AdminEditRoom';

interface AdminPanelProps {
  isAdmin: boolean;
  showAllBookings: boolean;
  token: string | null;
  onMessage: (msg: string) => void;
  onRefresh: () => Promise<void>;
}

export interface AdminPanelHandle {
  startEdit: (roomId: string, roomName: string, capacity: number) => void;
  cancelEdit: () => void;
  handleDeleteRoom: (roomId: string) => Promise<void>;
}

const API_URL = 'http://localhost:5080/api';

export const AdminPanel = forwardRef<AdminPanelHandle, AdminPanelProps>(
  (
    { isAdmin, showAllBookings, token, onMessage, onRefresh },
    ref
  ) => {
    // Room creation state
    const [newRoomName, setNewRoomName] = useState('');
    const [newRoomCapacity, setNewRoomCapacity] = useState(10);

    // Room editing state
    const [editingRoomId, setEditingRoomId] = useState<string | null>(null);
    const [editRoomName, setEditingRoomName] = useState('');
    const [editRoomCapacity, setEditingRoomCapacity] = useState(10);

    // Expose imperative handle for RoomGrid to trigger edit
    useImperativeHandle(ref, () => ({
      startEdit: (roomId: string, roomName: string, capacity: number) => {
        setEditingRoomId(roomId);
        setEditingRoomName(roomName);
        setEditingRoomCapacity(capacity);
      },
      cancelEdit: () => {
        setEditingRoomId(null);
      },
      handleDeleteRoom: handleDeleteRoom,
    }));

    // Handle create room
    const handleCreateRoom = async (e: React.FormEvent) => {
      e.preventDefault();
      onMessage('');
      try {
        const res = await fetch(`${API_URL}/rooms`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            name: newRoomName,
            capacity: newRoomCapacity,
          }),
        });

        if (!res.ok) throw new Error('Failed to create room.');
        onMessage('New meeting room created successfully!');
        setNewRoomName('');
        setNewRoomCapacity(10);
        await onRefresh();
        setTimeout(() => onMessage(''), 5000);
      } catch (err: any) {
        onMessage(err.message);
        setTimeout(() => onMessage(''), 5000);
      }
    };

    // Handle update room
    const handleUpdateRoom = async (e: React.FormEvent) => {
      e.preventDefault();
      if (!editingRoomId) return;
      onMessage('');
      try {
        const res = await fetch(`${API_URL}/rooms/${editingRoomId}`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            name: editRoomName,
            capacity: editRoomCapacity,
          }),
        });

        if (!res.ok) throw new Error('Failed to update room.');
        onMessage('Meeting room parameters successfully updated.');
        setEditingRoomId(null);
        await onRefresh();
        setTimeout(() => onMessage(''), 5000);
      } catch (err: any) {
        onMessage(err.message);
        setTimeout(() => onMessage(''), 5000);
      }
    };

    // Handle delete room
    const handleDeleteRoom = async (roomId: string) => {
      if (
        !window.confirm(
          'Are you sure you want to delete this room completely?'
        )
      )
        return;
      onMessage('');
      try {
        const res = await fetch(`${API_URL}/rooms/${roomId}`, {
          method: 'DELETE',
          headers: { Authorization: `Bearer ${token}` },
        });

        if (!res.ok) throw new Error('Failed to delete room.');
        onMessage('Room deleted successfully.');
        await onRefresh();
        setTimeout(() => onMessage(''), 5000);
      } catch (err: any) {
        onMessage(err.message);
        setTimeout(() => onMessage(''), 5000);
      }
    };

    if (!isAdmin) return null;

    return (
      <>
        {!showAllBookings && (
          <AdminCreateRoom
            newRoomName={newRoomName}
            newRoomCapacity={newRoomCapacity}
            onNameChange={setNewRoomName}
            onCapacityChange={setNewRoomCapacity}
            onSubmit={handleCreateRoom}
          />
        )}

        {editingRoomId && (
          <AdminEditRoom
            editRoomName={editRoomName}
            editRoomCapacity={editRoomCapacity}
            onNameChange={setEditingRoomName}
            onCapacityChange={setEditingRoomCapacity}
            onSubmit={handleUpdateRoom}
            onCancel={() => setEditingRoomId(null)}
          />
        )}
      </>
    );
  }
);
