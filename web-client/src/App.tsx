import { useState, useEffect } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

interface CamPoint {
  theta: number;
  s: number;
  v: number;
  a: number;
  j: number;
}

interface Point {
  id: number;
  master: number;
  slave: number;
}

function App() {
  // 1. State for Inputs
  const [points, setPoints] = useState<Point[]>([
    { id: 1, master: 0, slave: 0 },
    { id: 2, master: 180, slave: 100 },
    { id: 3, master: 360, slave: 0 }
  ]);
  const [resolution, setResolution] = useState<number>(200);
  
  // 2. State for Data
  const [data, setData] = useState<CamPoint[]>([]);
  // const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Helper: Sort points by master position
  const sortedPoints = [...points].sort((a, b) => a.master - b.master);

  // 3. Fetch Data from Python Backend
  useEffect(() => {
    const fetchData = async () => {
      // Basic Validation
      if (points.length < 2) {
        setData([]);
        return;
      }

      // setLoading(true);
      setError(null);

      try {
        const response = await fetch('http://localhost:8000/calculate_sequence', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            points: sortedPoints.map(p => ({ master: p.master, slave: p.slave })),
            resolution
          }),
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.detail || 'Failed to fetch data');
        }

        const result: CamPoint[] = await response.json();
        
        // Process data for display (rounding)
        const processedData = result.map(p => ({
          ...p,
          s: parseFloat(p.s.toFixed(4)),
          v: parseFloat(p.v.toFixed(4)),
          a: parseFloat(p.a.toFixed(4)),
          j: parseFloat(p.j.toFixed(4)),
          theta: parseFloat(p.theta.toFixed(2))
        }));

        setData(processedData);
      } catch (err: any) {
        console.error("Error fetching data:", err);
        setError(err.message);
      } finally {
        // setLoading(false);
      }
    };

    // Debounce the API call slightly to avoid too many requests while typing
    const timeoutId = setTimeout(fetchData, 300);
    return () => clearTimeout(timeoutId);

  }, [points, resolution]); // Depend on points changes

  // Point Management
  const addPoint = () => {
    const lastPoint = sortedPoints[sortedPoints.length - 1];
    const newMaster = lastPoint ? lastPoint.master + 90 : 0;
    const newSlave = lastPoint ? lastPoint.slave : 0;
    setPoints([...points, { id: Date.now(), master: newMaster, slave: newSlave }]);
  };

  const removePoint = (id: number) => {
    if (points.length <= 2) return;
    setPoints(points.filter(p => p.id !== id));
  };

  const updatePoint = (id: number, field: 'master' | 'slave', value: number) => {
    setPoints(points.map(p => p.id === id ? { ...p, [field]: value } : p));
  };

  // 4. Export to CSV
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
          <h1 className="text-3xl font-bold text-indigo-600">Quintic <span className="text-gray-500 text-lg font-normal">Multi-Segment Editor</span></h1>
          <p className="text-gray-500 mt-2">Design complex cam profiles by adding multiple control points (Segments use 5th-order Polynomial R-R).</p>
        </header>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            
            {/* Left Panel: Point Editor */}
            <div className="lg:col-span-1 space-y-6">
                <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
                    <div className="flex justify-between items-center mb-4">
                        <h2 className="text-lg font-semibold text-gray-700">Control Points</h2>
                        <button 
                            onClick={addPoint}
                            className="bg-indigo-100 hover:bg-indigo-200 text-indigo-700 text-sm font-medium py-1 px-3 rounded transition-colors"
                        >
                            + Add Point
                        </button>
                    </div>
                    
                    <div className="space-y-3 max-h-[500px] overflow-y-auto pr-2">
                        {sortedPoints.map((point) => (
                            <div key={point.id} className="flex items-center gap-2 bg-gray-50 p-3 rounded border border-gray-100 relative group">
                                <div className="flex-1">
                                    <label className="text-xs text-gray-500 block mb-1">Master (°)</label>
                                    <input 
                                        type="number" 
                                        value={point.master} 
                                        onChange={(e) => updatePoint(point.id, 'master', Number(e.target.value))}
                                        className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-indigo-500"
                                    />
                                </div>
                                <div className="flex-1">
                                    <label className="text-xs text-gray-500 block mb-1">Slave (mm)</label>
                                    <input 
                                        type="number" 
                                        value={point.slave} 
                                        onChange={(e) => updatePoint(point.id, 'slave', Number(e.target.value))}
                                        className="w-full border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-indigo-500"
                                    />
                                </div>
                                {points.length > 2 && (
                                    <button 
                                        onClick={() => removePoint(point.id)}
                                        className="text-gray-400 hover:text-red-500 p-1 opacity-0 group-hover:opacity-100 transition-opacity absolute -right-2 -top-2 bg-white rounded-full shadow-sm border border-gray-200"
                                        title="Remove Point"
                                    >
                                        <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                        </svg>
                                    </button>
                                )}
                                <div className="absolute left-0 top-0 bottom-0 w-1 bg-indigo-500 rounded-l opacity-0 group-hover:opacity-100 transition-opacity"></div>
                            </div>
                        ))}
                    </div>

                    <div className="mt-6 pt-6 border-t border-gray-100">
                        <label className="text-sm font-medium text-gray-700 block mb-2">Total Resolution (Points)</label>
                        <input 
                            type="number" 
                            value={resolution} 
                            onChange={e => setResolution(Number(e.target.value))}
                            className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                        />
                    </div>
                     <div className="mt-4">
                        <button 
                            onClick={handleExport}
                            disabled={data.length === 0}
                            className={`w-full font-bold py-2 px-4 rounded transition-colors flex items-center justify-center gap-2 ${data.length === 0 ? 'bg-gray-400 cursor-not-allowed text-gray-200' : 'bg-indigo-600 hover:bg-indigo-700 text-white'}`}
                        >
                        <span>Download Sequence CSV</span>
                        </button>
                    </div>
                </div>

                 {/* Status / Error Message */}
                {error && (
                <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative text-sm" role="alert">
                    <strong className="font-bold">Error: </strong>
                    <span className="block">{error}</span>
                </div>
                )}
            </div>

            {/* Right Panel: Visualization */}
            <div className="lg:col-span-2 space-y-6">
                {/* Charts Grid */}
                <div className="grid grid-cols-1 gap-6">
                
                {/* Displacement */}
                <ChartCard title="Displacement (s)" color="#4f46e5" dataKey="s" data={data} unit="mm" height={250} />
                
                {/* Velocity */}
                <ChartCard title="Velocity (v)" color="#0ea5e9" dataKey="v" data={data} unit="mm/deg" height={250} />

                {/* Acceleration */}
                <ChartCard title="Acceleration (a)" color="#10b981" dataKey="a" data={data} unit="mm/deg²" height={250} />

                {/* Jerk */}
                <ChartCard title="Jerk (j)" color="#f59e0b" dataKey="j" data={data} unit="mm/deg³" height={250} />
                
                </div>
            </div>

        </div>

      </div>
    </div>
  )
}

// Reusable Chart Component
const ChartCard = ({ title, color, dataKey, data, unit, height }: { title: string, color: string, dataKey: string, data: any[], unit: string, height: number }) => (
  <div className="bg-white p-4 rounded-lg shadow-sm border border-gray-200 flex flex-col" style={{ height: height }}>
    <h3 className="text-md font-semibold text-gray-700 mb-2 flex justify-between">
      {title} <span className="text-xs text-gray-400 self-center">{unit}</span>
    </h3>
    <div className="flex-1 w-full min-h-0">
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e5e7eb" />
          <XAxis 
            dataKey="theta" 
            type="number" 
            domain={['dataMin', 'dataMax']} 
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
