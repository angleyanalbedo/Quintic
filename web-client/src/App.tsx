import { useState, useEffect } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { MotionLawType } from './types';
import type { Project, Segment, CamPoint } from './types';

function App() {
  // 1. Project State
  const [project, setProject] = useState<Project>({
    config: {
      masterVelocity: 60,
      resolution: 360
    },
    segments: [
      { id: '1', motionLaw: MotionLawType.POLY5, masterStart: 0, masterEnd: 90, slaveStart: 0, slaveEnd: 50 },
      { id: '2', motionLaw: MotionLawType.POLY5, masterStart: 90, masterEnd: 180, slaveStart: 50, slaveEnd: 50 },
      { id: '3', motionLaw: MotionLawType.POLY5, masterStart: 180, masterEnd: 360, slaveStart: 50, slaveEnd: 0 }
    ]
  });
  
  // 2. Data State
  const [data, setData] = useState<CamPoint[]>([]);
  const [error, setError] = useState<string | null>(null);

  // 3. API Calculation
  useEffect(() => {
    const fetchData = async () => {
      // Basic validation
      if (project.segments.length === 0) return;

      try {
        // Map frontend CamelCase to backend snake_case
        const payload = {
            config: {
                master_velocity: project.config.masterVelocity,
                resolution: project.config.resolution
            },
            segments: project.segments.map(s => ({
                id: s.id,
                motion_law: s.motionLaw,
                master_start: s.masterStart,
                master_end: s.masterEnd,
                slave_start: s.slaveStart,
                slave_end: s.slaveEnd,
                // Defaults for MVP
                start_velocity: 0,
                end_velocity: 0,
                start_acceleration: 0,
                end_acceleration: 0
            }))
        };

        const response = await fetch('http://localhost:8000/api/v1/calculate_project', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload),
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.detail || 'Failed to calculate');
        }

        const result: CamPoint[] = await response.json();
        
        // Process data
        setData(result.map(p => ({
          ...p,
          s: parseFloat(p.s.toFixed(4)),
          v: parseFloat(p.v.toFixed(4)),
          a: parseFloat(p.a.toFixed(4)),
          j: parseFloat(p.j.toFixed(4)),
          theta: parseFloat(p.theta.toFixed(2))
        })));
        setError(null);

      } catch (err: any) {
        console.error("Calculation error:", err);
        setError(err.message);
      }
    };

    const timeoutId = setTimeout(fetchData, 300);
    return () => clearTimeout(timeoutId);

  }, [project]);

  // Segment Management
  const addSegment = () => {
    const lastSeg = project.segments[project.segments.length - 1];
    const newStart = lastSeg ? lastSeg.masterEnd : 0;
    const newSlaveStart = lastSeg ? lastSeg.slaveEnd : 0;
    
    const newSegment: Segment = {
        id: Date.now().toString(),
        motionLaw: MotionLawType.POLY5,
        masterStart: newStart,
        masterEnd: newStart + 90,
        slaveStart: newSlaveStart,
        slaveEnd: newSlaveStart
    };
    
    setProject({ ...project, segments: [...project.segments, newSegment] });
  };

  const updateSegment = (id: string, field: keyof Segment, value: any) => {
    setProject({
        ...project,
        segments: project.segments.map(s => s.id === id ? { ...s, [field]: value } : s)
    });
  };

  const removeSegment = (id: string) => {
      if(project.segments.length <= 1) return;
      setProject({
          ...project,
          segments: project.segments.filter(s => s.id !== id)
      });
  };

  const handleExport = () => {
    if (!data.length) return;
    const headers = ['Theta', 'S', 'V', 'A', 'J'];
    const csvContent = [
      headers.join(','),
      ...data.map(row => `${row.theta},${row.s},${row.v},${row.a},${row.j}`)
    ].join('\n');
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', 'quintic_project.csv');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="min-h-screen bg-gray-50 p-8 font-sans text-gray-800">
      <div className="max-w-7xl mx-auto space-y-6">
        
        <header className="flex justify-between items-center border-b pb-4">
          <div>
            <h1 className="text-2xl font-bold text-indigo-600">Quintic <span className="text-gray-500 text-lg font-normal">Project Editor</span></h1>
            <p className="text-xs text-gray-400 mt-1">VDI 2143 Multi-Segment Design</p>
          </div>
          <button onClick={handleExport} className="bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium py-2 px-4 rounded transition-colors">
            Export CSV
          </button>
        </header>

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
            
            {/* Left: Segment List */}
            <div className="lg:col-span-4 space-y-4">
                <div className="bg-white p-4 rounded-lg shadow-sm border border-gray-200">
                    <div className="flex justify-between items-center mb-4">
                        <h2 className="text-sm font-semibold text-gray-700">Motion Segments</h2>
                        <button onClick={addSegment} className="text-indigo-600 hover:text-indigo-800 text-xs font-bold uppercase tracking-wide">
                            + Add Segment
                        </button>
                    </div>
                    
                    <div className="space-y-3 max-h-[600px] overflow-y-auto pr-2">
                        {project.segments.map((seg, idx) => (
                            <div key={seg.id} className="bg-gray-50 p-3 rounded border border-gray-100 text-sm space-y-2 relative group">
                                <div className="flex justify-between items-center">
                                    <span className="font-mono text-xs text-gray-400">#{idx + 1}</span>
                                    <select 
                                        value={seg.motionLaw}
                                        onChange={(e) => updateSegment(seg.id, 'motionLaw', e.target.value)}
                                        className="bg-white border border-gray-200 rounded text-xs py-1 px-2 focus:outline-none focus:border-indigo-500"
                                    >
                                        <option value={MotionLawType.POLY5}>Poly 5 (3-4-5)</option>
                                        <option value={MotionLawType.MODIFIED_SINE} disabled>Modified Sine (Soon)</option>
                                        <option value={MotionLawType.CYCLOIDAL} disabled>Cycloidal (Soon)</option>
                                    </select>
                                    {project.segments.length > 1 && (
                                        <button onClick={() => removeSegment(seg.id)} className="text-gray-300 hover:text-red-500 transition-colors">
                                            ×
                                        </button>
                                    )}
                                </div>
                                
                                <div className="grid grid-cols-2 gap-2">
                                    <div>
                                        <label className="text-[10px] text-gray-400 block uppercase">Master Start</label>
                                        <input type="number" value={seg.masterStart} onChange={(e) => updateSegment(seg.id, 'masterStart', Number(e.target.value))} className="w-full bg-white border border-gray-200 rounded px-2 py-1" />
                                    </div>
                                    <div>
                                        <label className="text-[10px] text-gray-400 block uppercase">Master End</label>
                                        <input type="number" value={seg.masterEnd} onChange={(e) => updateSegment(seg.id, 'masterEnd', Number(e.target.value))} className="w-full bg-white border border-gray-200 rounded px-2 py-1" />
                                    </div>
                                    <div>
                                        <label className="text-[10px] text-gray-400 block uppercase">Slave Start</label>
                                        <input type="number" value={seg.slaveStart} onChange={(e) => updateSegment(seg.id, 'slaveStart', Number(e.target.value))} className="w-full bg-white border border-gray-200 rounded px-2 py-1" />
                                    </div>
                                    <div>
                                        <label className="text-[10px] text-gray-400 block uppercase">Slave End</label>
                                        <input type="number" value={seg.slaveEnd} onChange={(e) => updateSegment(seg.id, 'slaveEnd', Number(e.target.value))} className="w-full bg-white border border-gray-200 rounded px-2 py-1" />
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                </div>
                
                {error && (
                    <div className="bg-red-50 text-red-600 p-3 rounded text-xs border border-red-100">
                        {error}
                    </div>
                )}
            </div>

            {/* Right: Visualization */}
            <div className="lg:col-span-8 space-y-4">
                <div className="grid grid-cols-1 gap-4">
                    <ChartCard title="Displacement (S)" dataKey="s" data={data} color="#4f46e5" unit="mm" />
                    <ChartCard title="Velocity (V)" dataKey="v" data={data} color="#0ea5e9" unit="mm/deg" />
                    <ChartCard title="Acceleration (A)" dataKey="a" data={data} color="#10b981" unit="mm/deg²" />
                    <ChartCard title="Jerk (J)" dataKey="j" data={data} color="#f59e0b" unit="mm/deg³" />
                </div>
            </div>
        </div>
      </div>
    </div>
  )
}

const ChartCard = ({ title, dataKey, data, color, unit }: any) => (
    <div className="bg-white p-3 rounded-lg border border-gray-200 shadow-sm h-48 flex flex-col">
        <div className="flex justify-between items-center mb-2">
            <span className="text-xs font-semibold text-gray-600">{title}</span>
            <span className="text-[10px] text-gray-400 bg-gray-50 px-2 py-0.5 rounded">{unit}</span>
        </div>
        <div className="flex-1 min-h-0">
            <ResponsiveContainer width="100%" height="100%">
                <LineChart data={data}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                    <XAxis dataKey="theta" type="number" domain={['dataMin', 'dataMax']} hide />
                    <YAxis width={30} tick={{fontSize: 10}} axisLine={false} tickLine={false} />
                    <Tooltip contentStyle={{fontSize: '12px', padding: '4px 8px'}} itemStyle={{padding: 0}} />
                    <Line type="monotone" dataKey={dataKey} stroke={color} strokeWidth={1.5} dot={false} isAnimationActive={false} />
                </LineChart>
            </ResponsiveContainer>
        </div>
    </div>
);

export default App
