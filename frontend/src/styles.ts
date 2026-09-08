// Centralized style definitions for MeetingRoom app
// All inline styles from App.tsx extracted into organized structures

export const styles = {
  // Authentication
  authContainer: {
    maxWidth: '400px',
    margin: '100px auto'
  } as const,
  
  authHeading: {
    marginTop: 0
  } as const,

  authError: {
    color: 'red',
    fontSize: '0.875rem'
  } as const,

  authLabel: {
    fontWeight: '500' as const
  } as const,

  authButton: {
    width: '100%',
    marginTop: '0.5rem'
  } as const,

  authToggleText: {
    textAlign: 'center' as const,
    marginTop: '1.5rem',
    fontSize: '0.875rem',
    color: '#4b5563'
  } as const,

  authToggleLink: {
    color: '#2563eb',
    cursor: 'pointer' as const,
    fontWeight: 'bold' as const,
    textDecoration: 'underline' as const
  } as const,

  // Header
  headerContainer: {
    display: 'flex' as const,
    justifyContent: 'space-between' as const,
    alignItems: 'center' as const,
    marginBottom: '2rem',
    borderBottom: '1px solid #e5e7eb',
    paddingBottom: '1rem'
  } as const,

  headerTitle: {
    margin: 0,
    fontSize: '1.75rem'
  } as const,

  headerSubtitle: {
    margin: '0.25rem 0 0 0',
    color: '#6b7280'
  } as const,

  headerRole: {
    color: '#2563eb'
  } as const,

  headerButtonRow: {
    display: 'flex' as const,
    gap: '0.5rem'
  } as const,

  // Message notification
  messageNotification: {
    marginBottom: '1.5rem',
    backgroundColor: '#eff6ff',
    color: '#1e40af',
    fontWeight: '500' as const,
    position: 'absolute' as const,
    top: '20px',
    right: '50px'
  } as const,

  // Admin panels
  adminCreatePanel: {
    marginBottom: '2rem',
    backgroundColor: '#f9fafb',
    border: '1px dashed #2563eb'
  } as const,

  adminCreateTitle: {
    marginTop: 0,
    color: '#2563eb',
    fontSize: '1.25rem'
  } as const,

  adminEditPanel: {
    marginBottom: '2rem',
    backgroundColor: '#fffbeb',
    border: '1px solid #d97706'
  } as const,

  adminEditTitle: {
    marginTop: 0,
    color: '#d97706',
    fontSize: '1.25rem'
  } as const,

  formRow: {
    display: 'flex' as const,
    gap: '1rem',
    alignItems: 'flex-end' as const
  } as const,

  formFieldFull: {
    flex: 1
  } as const,

  formFieldSmall: {
    width: '150px'
  } as const,

  formLabel: {
    fontSize: '0.875rem',
    fontWeight: '500' as const
  } as const,

  formInput: {
    margin: '0.25rem 0 0 0'
  } as const,

  formButton: {
    height: '42px'
  } as const,

  formButtonSubmit: {
    height: '42px',
    backgroundColor: '#d97706'
  } as const,

  formButtonCancel: {
    height: '42px',
    backgroundColor: '#9ca3af'
  } as const,

  // Audit Log / Table
  auditTitle: {
    marginTop: 0,
    color: '#1f2937'
  } as const,

  table: {
    width: '100%',
    borderCollapse: 'collapse' as const,
    marginTop: '1rem'
  } as const,

  tableHeader: {
    textAlign: 'left' as const,
    borderBottom: '2px solid #e5e7eb',
    backgroundColor: '#f9fafb'
  } as const,

  tableHeaderCell: {
    padding: '0.75rem'
  } as const,

  tableRow: {
    borderBottom: '1px solid #e5e7eb'
  } as const,

  tableCell: {
    padding: '0.75rem'
  } as const,

  tableCellBold: {
    padding: '0.75rem',
    fontWeight: 'bold' as const
  } as const,

  tableCellStatus: {
    padding: '0.75rem',
    color: '#111827',
    fontWeight: '600' as const
  } as const,

  tableStatusBadge: {
    backgroundColor: '#e0f2fe',
    color: '#0369a1',
    padding: '0.25rem 0.5rem',
    borderRadius: '4px',
    fontSize: '0.875rem'
  } as const,

  tableEmptyRow: {
    padding: '2rem',
    textAlign: 'center' as const,
    color: '#9ca3af'
  } as const,

  // Room Grid
  roomCardContainer: {
    position: 'relative' as const
  } as const,

  roomTitle: {
    margin: '0 120px 0.25rem 0',
    fontSize: '1.25rem'
  } as const,

  roomCapacity: {
    fontSize: '0.875rem',
    color: '#4b5563'
  } as const,

  adminButtonRow: {
    position: 'absolute' as const,
    top: '10px',
    right: '10px',
    display: 'flex' as const,
    gap: '0.25rem'
  } as const,

  adminEditButton: {
    backgroundColor: '#fef3c7',
    color: '#d97706',
    padding: '0.25rem 0.5rem',
    fontSize: '0.75rem',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer' as const,
    fontWeight: 'bold' as const
  } as const,

  adminDeleteButton: {
    backgroundColor: '#fee2e2',
    color: '#ef4444',
    padding: '0.25rem 0.5rem',
    fontSize: '0.75rem',
    border: 'none',
    borderRadius: '4px',
    cursor: 'pointer' as const,
    fontWeight: 'bold' as const
  } as const,

  slotGrid: {
    display: 'flex' as const,
    flexWrap: 'wrap' as const,
    gap: '0.5rem',
    marginTop: '1rem'
  } as const,

  slotButton: {
    color: '#ffffff',
    padding: '0.5rem 0.75rem',
    border: 'none',
    borderRadius: '4px',
    fontWeight: '600' as const,
    transition: 'all 0.2s ease',
    display: 'flex' as const,
    alignItems: 'center' as const,
    gap: '0.25rem'
  } as const,

  // Slot colors - 07:00-18:00 business hours color matrix (preserved exactly)
  slotColorFree: '#10b981',      // green - available
  slotColorOwn: '#ef4444',       // red - own booking or admin
  slotColorOther: '#9ca3af',     // gray - booked by others (disabled)
} as const;
