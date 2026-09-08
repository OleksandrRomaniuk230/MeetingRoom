import { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';

interface TimeSlot {
  id: string;
  startTime: string;
  endTime: string;
  isBooked: boolean;
  bookedByUserId?: string;
  bookedByUsername?:string;
}

interface Room {
  id: string;
  name: string;
  capacity: number;
  slots?: TimeSlot[];
 
}


export default function App() {
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [userRole, setUserRole] = useState<string | null>(localStorage.getItem('userRole'));
  const [currentUserId, setCurrentUserId] = useState<string | null>(localStorage.getItem('userId'));
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [authError, setAuthError] = useState('');
  const [isSignUp, setIsSignUp] = useState(false); 
  const [email, setEmail] = useState('');          

  const [rooms, setRooms] = useState<Room[]>([]);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState('');

  // Admin management UI states
  const [newRoomName, setNewRoomName] = useState('');
  const [newRoomCapacity, setNewRoomCapacity] = useState(10);
  const [editingRoomId, setEditingRoomId] = useState<string | null>(null);
  const [editRoomName, setEditingRoomName] = useState('');
  const [editRoomCapacity, setEditingRoomCapacity] = useState(10);
  const [showAllBookings, setShowAllBookings] = useState(false);

  const API_URL = 'http://localhost:5080/api';

  useEffect(() => {
    if (token) {
      fetchRoomsAndSlots();
    }
  }, [token]);

  useEffect(() => {
    if (!token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5080/api/hubs/bookings', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    connection.on('SlotStatusChanged', (slotId: string, isBooked: boolean) => {
      setRooms(prevRooms => 
        prevRooms.map(room => ({
          ...room,
          slots: room.slots?.map(slot => 
            slot.id === slotId ? { ...slot, isBooked: isBooked } : slot
          )
        }))
      );
    });

    connection.start().catch(err => console.error('SignalR error: ', err));

    return () => { connection.stop(); };
  }, [token]);

  const fetchRoomsAndSlots = async () => {
    setLoading(true);
    try {
      const res = await fetch(`${API_URL}/rooms`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (!res.ok) throw new Error('Failed to load meeting rooms.');
      const roomsList: Room[] = await res.json();

      const completedData = await Promise.all(
        roomsList.map(async (room) => {
          const slotRes = await fetch(`${API_URL}/rooms/${room.id}/slots`, {
            headers: { Authorization: `Bearer ${token}` }
          });
          const slots = slotRes.ok ? await slotRes.json() : [];
          return { ...room, slots };
        })
      );
      setRooms(completedData);
    } catch (err: any) {
      setMessage(err.message);
    } finally {
      setLoading(false);
    }
  };

const executeLoginFlow = (accessToken: string) => {
    const parts = accessToken.split('.');
    const payload = JSON.parse(atob(parts[1].replace(/-/g, '+').replace(/_/g, '/')));
    
    const role = String(payload.roles || payload.role || 'regular user').toLowerCase();
    const userId = payload.uid || payload.sub || ''; 

    localStorage.setItem('token', accessToken);
    localStorage.setItem('userRole', role);
    localStorage.setItem('userId', userId);
    
    setToken(accessToken);
    setUserRole(role);
    setCurrentUserId(userId);
    setIsSignUp(false);
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setAuthError('');
    try {
      const res = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });

      if (!res.ok) {
        const errData = await res.json().catch(() => ({}));
        throw new Error(errData.detail || errData.title || 'Invalid username or password.');
      }
      
      const data = await res.json();
      executeLoginFlow(data.accessToken);
    } catch (err: any) {
      setAuthError(err.message);
    }
  };

  const handleSignUp = async (e: React.FormEvent) => {
    e.preventDefault();
    setAuthError('');
    try {
      const res = await fetch(`${API_URL}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, email, password })
      });

      if (!res.ok) {
        const errData = await res.json().catch(() => ({}));
        
        if (errData.errors) {
          const errorMessages = Object.values(errData.errors).flat().join(' ');
          throw new Error(errorMessages);
        }
        
        throw new Error(errData.detail || errData.title || 'Registration failed.');
      }

      const loginRes = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });
      
      if (!loginRes.ok) throw new Error('Registered successfully, but auto-login failed. Sign in manually.');
      const data = await loginRes.json();
      
      executeLoginFlow(data.accessToken);
    } catch (err: any) {
      setAuthError(err.message);
    }
  };

  const handleCancelSlot = async (slotId: string) => {
    if (!window.confirm('Do you want to cancel this booking?')) return;
    setMessage('');
    try {
      const res = await fetch(`${API_URL}/bookings/${slotId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` }
      });

      if (res.status === 403) {
        throw new Error('Forbidden! You can only cancel your own reservations.');
      }
      if (!res.ok) throw new Error('Failed to cancel the booking.');

      setMessage('Booking successfully canceled.');
      setTimeout(() => setMessage(''), 5000);
      fetchRoomsAndSlots();
    } catch (err: any) {
      setMessage(err.message);
      setTimeout(() => setMessage(''), 5000);
    }
  };

  const handleBookSlot = async (slotId: string) => {
    setMessage('');
    try {
      const res = await fetch(`${API_URL}/bookings`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        body: JSON.stringify({ timeSlotId: slotId })
      });
      
      if (res.status === 409) {
        throw new Error('Conflict! This slot has just been reserved by another user.');
      }
      if (!res.ok) throw new Error('An error occurred while processing the reservation.');
      
      setMessage('Room successfully booked!');
      setTimeout(() => setMessage(''), 5000);
      fetchRoomsAndSlots(); 
    } catch (err: any) { 
      setMessage(err.message); 
      setTimeout(() => setMessage(''), 5000);
    }
  };


  const handleCreateRoom = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage('');
    try {
      const res = await fetch(`${API_URL}/rooms`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify({ name: newRoomName, capacity: newRoomCapacity })
      });

      if (!res.ok) throw new Error('Failed to create room.');
      setMessage('New meeting room created successfully!');
      setNewRoomName('');
      fetchRoomsAndSlots();
      setTimeout(() => setMessage(''), 5000);
    } catch (err: any) {
      setMessage(err.message);
      setTimeout(() => setMessage(''), 5000);
    }
  };

  const handleUpdateRoom = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingRoomId) return;
    setMessage('');
    try {
      const res = await fetch(`${API_URL}/rooms/${editingRoomId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify({ name: editRoomName, capacity: editRoomCapacity })
      });

      if (!res.ok) throw new Error('Failed to update room.');
      setMessage('Meeting room parameters successfully updated.');
      setEditingRoomId(null);
      fetchRoomsAndSlots();
      setTimeout(() => setMessage(''), 5000);
    } catch (err: any) {
      setMessage(err.message);
      setTimeout(() => setMessage(''), 5000);
    }
  };

  const handleDeleteRoom = async (roomId: string) => {
    if (!window.confirm('Are you sure you want to delete this room completely?')) return;
    setMessage('');
    try {
      const res = await fetch(`${API_URL}/rooms/${roomId}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${token}` }
      });

      if (!res.ok) throw new Error('Failed to delete room.');
      setMessage('Room deleted successfully.');
      fetchRoomsAndSlots();
      setTimeout(() => setMessage(''), 5000);
    } catch (err: any) {
      setMessage(err.message);
      setTimeout(() => setMessage(''), 5000);
    }
  };

  const handleLogout = () => {
    localStorage.clear();
    setToken(null);
    setUserRole(null);
    setRooms([]);
  };

  const isAdmin = userRole === 'admin';

  const formatSlotLabel = (isoString: string): string => {
    try {
      const timePart = isoString.split('T')[1]; 
      return timePart.substring(0, 5); 
    } catch (e) {
      return "00:00";
    }
  };
  if (!token) {
    return (
      <div style={{ maxWidth: '400px', margin: '100px auto' }} className="card">
        <h2 style={{ marginTop: 0 }}>{isSignUp ? 'Create MeetingRoom Account' : 'MeetingRoom Sign In'}</h2>
        {authError && <p style={{ color: 'red', fontSize: '0.875rem' }}>{authError}</p>}
                <form onSubmit={isSignUp ? handleSignUp : handleLogin}>
          <label style={{ fontWeight: '500' }}>Username</label>
          <input type="text" value={username} onChange={e => setUsername(e.target.value)} required />
                    {isSignUp && (
            <>
              <label style={{ fontWeight: '500' }}>Email Address</label>
              <input type="email" value={email} onChange={e => setEmail(e.target.value)} required={isSignUp} />
            </>
          )}
          
          <label style={{ fontWeight: '500' }}>Password</label>
          <input type="password" value={password} onChange={e => setPassword(e.target.value)} required />
          
          <button type="submit" style={{ width: '100%', marginTop: '0.5rem' }}>
            {isSignUp ? 'Sign Up (Register)' : 'Sign In'}
          </button>
        </form>
        <p style={{ textAlign: 'center', marginTop: '1.5rem', fontSize: '0.875rem', color: '#4b5563' }}>
          {isSignUp ? 'Already have an account?' : "Don't have an account?"}{' '}
          <span 
            style={{ color: '#2563eb', cursor: 'pointer', fontWeight: 'bold', textDecoration: 'underline' }}
            onClick={() => { setIsSignUp(!isSignUp); setAuthError(''); }}
          >
            {isSignUp ? 'Sign In here' : 'Register/Sign Up here'}
          </span>
        </p>
      </div>
    );
  }


  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem', borderBottom: '1px solid #e5e7eb', paddingBottom: '1rem' }}>
        <div>
          <h1 style={{ margin: 0, fontSize: '1.75rem' }}>Room Booking Dashboard</h1>
          <p style={{ margin: '0.25rem 0 0 0', color: '#6b7280' }}>
            Logged in as: <strong style={{ color: '#2563eb' }}>{userRole}</strong> (Real-time Active 🟢)
          </p>
        </div>
        <div style={{ display: 'flex', gap: '0.5rem' }}>
          {isAdmin && (
            <button onClick={() => setShowAllBookings(!showAllBookings)} style={{ backgroundColor: '#4b5563' }}>
              {showAllBookings ? 'View Schedule Grid' : '🔍 View All Bookings (Audit)'}
            </button>
          )}
          <button onClick={handleLogout} style={{ backgroundColor: '#ef4444' }}>Sign Out</button>
        </div>
      </div>

      {message && <div className="card" style={{ marginBottom: '1.5rem', backgroundColor: '#eff6ff', color: '#1e40af', fontWeight: '500', position: 'absolute', top:'20px', right:'50px' }}>{message}</div>}

      {isAdmin && !showAllBookings && (
        <div className="card" style={{ marginBottom: '2rem', backgroundColor: '#f9fafb', border: '1px dashed #2563eb' }}>
          <h2 style={{ marginTop: 0, color: '#2563eb', fontSize: '1.25rem' }}>🛡️ Admin Management Panel (Create Resource)</h2>
          <form onSubmit={handleCreateRoom} style={{ display: 'flex', gap: '1rem', alignItems: 'flex-end' }}>
            <div style={{ flex: 1 }}>
              <label style={{ fontSize: '0.875rem', fontWeight: '500' }}>Room Name</label>
              <input type="text" value={newRoomName} onChange={e => setNewRoomName(e.target.value)} style={{ margin: '0.25rem 0 0 0' }} required />
            </div>
            <div style={{ width: '150px' }}>
              <label style={{ fontSize: '0.875rem', fontWeight: '500' }}>Capacity</label>
              <input type="number" value={newRoomCapacity} onChange={e => setNewRoomCapacity(Number(e.target.value))} style={{ margin: '0.25rem 0 0 0' }} min="1" required />
            </div>
            <button type="submit" style={{ height: '42px' }}>Create New Room</button>
          </form>
        </div>
      )}

      {isAdmin && editingRoomId && (
        <div className="card" style={{ marginBottom: '2rem', backgroundColor: '#fffbeb', border: '1px solid #d97706' }}>
          <h2 style={{ marginTop: 0, color: '#d97706', fontSize: '1.25rem' }}>✏️ Edit Resource Parameters</h2>
          <form onSubmit={handleUpdateRoom} style={{ display: 'flex', gap: '1rem', alignItems: 'flex-end' }}>
            <div style={{ flex: 1 }}>
              <label style={{ fontSize: '0.875rem', fontWeight: '500' }}>Updated Room Name</label>
              <input type="text" value={editRoomName} onChange={e => setEditingRoomName(e.target.value)} style={{ margin: '0.25rem 0 0 0' }} required />
            </div>
            <div style={{ width: '150px' }}>
              <label style={{ fontSize: '0.875rem', fontWeight: '500' }}>Updated Capacity</label>
              <input type="number" value={editRoomCapacity} onChange={e => setEditingRoomCapacity(Number(e.target.value))} style={{ margin: '0.25rem 0 0 0' }} min="1" required />
            </div>
            <button type="submit" style={{ height: '42px', backgroundColor: '#d97706' }}>Save Changes</button>
            <button type="button" style={{ height: '42px', backgroundColor: '#9ca3af' }} onClick={() => setEditingRoomId(null)}>Cancel</button>
          </form>
        </div>
      )}

      {isAdmin && showAllBookings ? (
        <div className="card">
          <h2 style={{ marginTop: 0, color: '#1f2937' }}>📋 Global Reservations Audit Log</h2>
          <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '1rem' }}>
            <thead>
              <tr style={{ textAlign: 'left', borderBottom: '2px solid #e5e7eb', backgroundColor: '#f9fafb' }}>
                <th style={{ padding: '0.75rem' }}>Meeting Room</th>
                <th style={{ padding: '0.75rem' }}>Time Interval</th>
                <th style={{ padding: '0.75rem' }}>Status</th>
              </tr>
            </thead>
            <tbody>
              {rooms.flatMap(r => (r.slots || []).filter(s => s.isBooked).map(slot => (
                <tr key={slot.id} style={{ borderBottom: '1px solid #e5e7eb' }}>
                  <td style={{ padding: '0.75rem', fontWeight: 'bold' }}>{r.name}</td>
                  <td style={{ padding: '0.75rem' }}>{formatSlotLabel(slot.startTime)} UTC</td>
                  <td style={{ padding: '0.75rem', color: '#111827', fontWeight: '600' }}>
                     {slot.bookedByUsername || 'admin'} 
                  </td>
                  <td style={{ padding: '0.75rem' }}><span style={{ backgroundColor: '#e0f2fe', color: '#0369a1', padding: '0.25rem 0.5rem', borderRadius: '4px', fontSize: '0.875rem' }}>Active Reservation</span></td>
                </tr>
              )))}
              {rooms.every(r => !(r.slots || []).some(s => s.isBooked)) && (
                <tr>
                  <td colSpan={3} style={{ padding: '2rem', textAlign: 'center', color: '#9ca3af' }}>No active bookings found across the ecosystem.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      ) : (
        <div>
          {loading && <p>Updating slot matrices...</p>}
          <div className="grid">
            {rooms.map(room => (
              <div key={room.id} className="card" style={{ position: 'relative' }}>
                {isAdmin && (
                  <div style={{ position: 'absolute', top: '10px', right: '10px', display: 'flex', gap: '0.25rem' }}>
                    <button 
                      onClick={() => { setEditingRoomId(room.id); setEditingRoomName(room.name); setEditingRoomCapacity(room.capacity); }}
                      style={{ backgroundColor: '#fef3c7', color: '#d97706', padding: '0.25rem 0.5rem', fontSize: '0.75rem', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}
                    >
                      Edit
                    </button>
                    <button 
                      onClick={() => handleDeleteRoom(room.id)}
                      style={{ backgroundColor: '#fee2e2', color: '#ef4444', padding: '0.25rem 0.5rem', fontSize: '0.75rem', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}
                    >
                      Delete
                    </button>
                  </div>
                )}

                <h3 style={{ margin: '0 120px 0.25rem 0', fontSize: '1.25rem' }}>{room.name}</h3>
                <span style={{ fontSize: '0.875rem', color: '#4b5563' }}>Capacity: {room.capacity} people</span>
<div className="slot-grid" style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem', marginTop: '1rem' }}>
  {room.slots?.map(slot => {
    const slotUserId = slot.bookedByUserId ? String(slot.bookedByUserId).toLowerCase() : '';
    const authedUserId = currentUserId ? String(currentUserId).toLowerCase() : '';
    const isOwnBooking = slotUserId !== '' && slotUserId === authedUserId;
    const isRedState = isAdmin || isOwnBooking;

    let buttonColor = '#10b981'; 
    let cursorStyle = 'pointer';

    if (slot.isBooked) {
      if (isRedState) {
        buttonColor = '#ef4444'; 
      } else {
        buttonColor = '#9ca3af';
        cursorStyle = 'not-allowed';
      }
    }

    const canClick = !slot.isBooked || isRedState;

    return (
      <button
        key={slot.id}
        className={`slot-btn ${slot.isBooked ? 'slot-booked' : 'slot-free'}`}
        style={{ 
          backgroundColor: buttonColor, 
          color: '#ffffff',
          cursor: cursorStyle,
          padding: '0.5rem 0.75rem',
          border: 'none',
          borderRadius: '4px',
          fontWeight: '600',
          transition: 'all 0.2s ease',
          display: 'flex',
          alignItems: 'center',
          gap: '0.25rem'
        }}

        onClick={() => {
          if (!canClick) return;
          if (slot.isBooked) {
            handleCancelSlot(slot.id);
          } else {
            handleBookSlot(slot.id);
          }
        }}
      >
        {formatSlotLabel(slot.startTime)}
      </button>
    );
  })}
</div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
