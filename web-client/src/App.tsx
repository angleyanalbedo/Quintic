import { useState, useMemo } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { Polynomial5 } from './core/Polynomial5';

function App() {
  // 1. State for Inputs
  const [masterStart, setMasterStart] = useState<number>(0);
  const [masterEnd, setMasterEnd] = useState<number>(180);
  const [slaveStart, setSlaveStart] = useState<number>(0);
  const [slaveEnd, setSlaveEnd] = useState<number>(100);
  const [resolution, setResolution] = useState<number>(100);

  // 2. Compute Data (Reactive)
  const data = useMemo(() => {
    // Basic Validation
    if (masterEnd <= masterStart) return [];

    const poly = new Polynomial5(masterStart, masterEnd, slaveStart, slaveEnd);
    return poly.generateTable(resolution).map(p => ({
      ...p,
      // Round for display cleanliness
      s: parseFloat(p.s.toFixed(4)),
      v: parseFloat(p.v.toFixed(4)),
      a: parseFloat(p.a.toFixed(4)),
      j: parseFloat(p.j.toFixed(4)),
      theta: parseFloat(p.theta.toFixed(2))
    }));
  }, [masterStart, masterEnd, slaveStart, slaveEnd, resolution]);

  // 3. Export to CSV
  const handleExport = () => {
    if (!data.length) return;

    const headers = ['Master(deg)', 'Slave(mm)', 'Velocity', 'Acceleration', 'Jerk'];
    const csvContent = [
      headers.join(','),
      ...data.map(row => `${row.theta},${row.s},${row.v},${row.a},${row.j}`)
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', 'cam_profile_quintic.csv');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="min-h-screen bg-gray-50 p-8 font-sans text-gray-800">
      <div className="max-w-7xl mx-auto space-y-8">
        
        {/* Header */}
        <header className="border-b pb-4 mb-8">
          <h1 className="text-3xl font-bold text-indigo-600">Quintic <span className="text-gray-500 text-lg font-normal">MVP Editor</span></h1>
          <p className="text-gray-500 mt-2">VDI 2143 Compliant Cam Profile Generator (5th Order Polynomial)</p>
        </header>

        {/* Control Panel */}
        <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 grid grid-cols-1 md:grid-cols-5 gap-6 items-end">
          
          <div className="flex flex-col space-y-2">
            <label className="text-sm font-medium text-gray-700">Master Start (°)</label>
            <input 
              type="number" 
              value={masterStart} 
              onChange={e => setMasterStart(Number(e.target.value))}
              className="border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="flex flex-col space-y-2">
            <label className="text-sm font-medium text-gray-700">Master End (°)</label>
            <input 
              type="number" 
              value={masterEnd} 
              onChange={e => setMasterEnd(Number(e.target.value))}
              className="border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="flex flex-col space-y-2">
            <label className="text-sm font-medium text-gray-700">Slave Start (mm)</label>
            <input 
              type="number" 
              value={slaveStart} 
              onChange={e => setSlaveStart(Number(e.target.value))}
              className="border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="flex flex-col space-y-2">
            <label className="text-sm font-medium text-gray-700">Slave End (mm)</label>
            <input 
              type="number" 
              value={slaveEnd} 
              onChange={e => setSlaveEnd(Number(e.target.value))}
              className="border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="flex flex-col space-y-2">
            <label className="text-sm font-medium text-gray-700">Points (Res)</label>
            <input 
              type="number" 
              value={resolution} 
              onChange={e => setResolution(Number(e.target.value))}
              className="border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div className="flex flex-col space-y-2">
             <button 
              onClick={handleExport}
              className="bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-2 px-4 rounded transition-colors flex items-center justify-center gap-2"
            >
              <span>Download CSV</span>
            </button>
          </div>
        </div>

        {/* Charts Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          
          {/* Displacement */}
          <ChartCard title="Displacement (s)" color="#4f46e5" dataKey="s" data={data} unit="mm" />
          
          {/* Velocity */}
          <ChartCard title="Velocity (v)" color="#0ea5e9" dataKey="v" data={data} unit="mm/deg" />

          {/* Acceleration */}
          <ChartCard title="Acceleration (a)" color="#10b981" dataKey="a" data={data} unit="mm/deg²" />

          {/* Jerk */}
          <ChartCard title="Jerk (j)" color="#f59e0b" dataKey="j" data={data} unit="mm/deg³" />
          
        </div>

      </div>
    </div>
  )
}

// Reusable Chart Component
const ChartCard = ({ title, color, dataKey, data, unit }: { title: string, color: string, dataKey: string, data: any[], unit: string }) => (
  <div className="bg-white p-4 rounded-lg shadow-sm border border-gray-200 h-80 flex flex-col">
    <h3 className="text-md font-semibold text-gray-700 mb-4 flex justify-between">
      {title} <span className="text-xs text-gray-400 self-center">{unit}</span>
    </h3>
    <div className="flex-1 w-full min-h-0">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e5e7eb" />
          <XAxis 
            dataKey="theta" 
            type="number" 
            domain={['auto', 'auto']} 
            tick={{fontSize: 12, fill: '#9ca3af'}}
            tickLine={false}
            axisLine={{stroke: '#e5e7eb'}}
          />
          <YAxis 
            tick={{fontSize: 12, fill: '#9ca3af'}} 
            tickLine={false}
            axisLine={{stroke: '#e5e7eb'}}
            width={40}
          />
          <Tooltip 
            contentStyle={{ borderRadius: '8px', border: 'none', boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)' }}
            labelStyle={{ color: '#6b7280', marginBottom: '4px' }}
          />
          <Line 
            type="monotone" 
            dataKey={dataKey} 
            stroke={color} 
            strokeWidth={2} 
            dot={false} 
            activeDot={{ r: 4 }} 
            isAnimationActive={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  </div>
);

export default App
