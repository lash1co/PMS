export const menu_config = [
  {
    id: '1',
    label: 'Management',
    submenu: [
      {
        id: '1.1',
        label: 'Doctors',
        urlRoute: '#'
      },
      {
        id: '1.2',
        label: 'Patients',
        urlRoute: '/patients'
      },
      {
        id: '1.3',
        label: 'Users',
        urlRoute: '#'
      }
    ]
  },
  {
    id: '2',
    label: 'Medical Records',
    submenu: [
      {
        id: '2.1',
        label: 'Appointments',
        urlRoute: '#'
      },
      {
        id: '2.2',
        label: 'Schedule (Agenda)',
        urlRoute: '/schedule'
      }
    ]
  }
]
