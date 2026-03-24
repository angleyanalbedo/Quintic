import { useState, useEffect } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import { MotionLawType, CoordinateMode, ReferenceType } from './types';
import type { Project, Segment, CamPoint } from './types';

function App() {
  // 1. Project State with Industrial Schema
  const [project, setProject] = useState<Project>({
    config: {
      masterVelocity: 60,
      resolution: 360
    },
    segments: [
      { 
          id: '1', 
          motionLaw: MotionLawType.POLY5, 
          coordinateMode: CoordinateMode.ABSOLUTE,
          referenceType: ReferenceType.MASTER,
          masterVal: 90, 
          slaveVal: 50 
      },
      { 
          id: '2', 
          motionLaw: MotionLawType.POLY5,
          coordinateMode: CoordinateMode.RELATIVE, // Relative Mode Example
          referenceType: ReferenceType.MASTER,
          masterVal: 90, 
          slaveVal: 0 // Dwell (Relative 0)
      },
      { 
          id: '3', 
          motionLaw: MotionLawType.POLY5,
          coordinateMode: CoordinateMode.ABSOLUTE,
          referenceType: ReferenceType.MASTER,
          masterVal: 360, 
          slaveVal: 0 
      }
    ]
  });
  
  // 2. Data State
  const [data, setData] = useState<CamPoint[]>([]);
  const [error, setError] = useState<string | null>(null);

  // 3. API Calculation
  useEffect(() => {
    const fetchData = async () => {
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
                coordinate_mode: s.coordinateMode,
                reference_type: s.referenceType,
                master_val: s.masterVal,
                slave_val: s.slaveVal,
                // Defaults for MVP
                start_velocity: 0,
                end_velocity: 0,
                start_acceleration: 0,
                end_acceleration: 0,
                lambda_param: 0.5
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
    // const lastSeg = project.segments[project.segments.length - 1];
    
    // Logic for new segment defaults depends on mode
    // For MVP simplicity, just add a relative segment
    const newSegment: Segment = {
        id: Date.now().toString(),
        motionLaw: MotionLawType.POLY5,
        coordinateMode: CoordinateMode.RELATIVE,
        referenceType: ReferenceType.MASTER,
        masterVal: 90, // Delta 90 deg
        slaveVal: 0    // Dwell
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
            <h1 className="text-2xl font-bold text-indigo-600">Quintic <span className="text-gray-500 text-lg font-normal">Industrial Editor</span></h1>
            <p className="text-xs text-gray-400 mt-1">VDI 2143 (Relative, Time-Based, Events)</p>
          </div>
          <div className="flex gap-4 items-center">
            <div className="text-xs text-gray-500">
                Master Vel: <input type="number" value={project.config.masterVelocity} onChange={(e) => setProject({...project, config: {...project.config, masterVelocity: Number(e.target.value)}})} className="w-12 border rounded px-1"/> RPM
            </div>
            <button onClick={handleExport} className="bg-indigo-600 hover:bg-indigo-700 text-white text-sm font-medium py-2 px-4 rounded transition-colors">
                Export CSV
            </button>
          </div>
        </header>

        <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
            
            {/* Left: Segment List */}
            <div className="lg:col-span-5 space-y-4">
                <div className="bg-white p-4 rounded-lg shadow-sm border border-gray-200">
                    <div className="flex justify-between items-center mb-4">
                        <h2 className="text-sm font-semibold text-gray-700">Motion Sequence</h2>
                        <button onClick={addSegment} className="text-indigo-600 hover:text-indigo-800 text-xs font-bold uppercase tracking-wide">
                            + Add Segment
                        </button>
                    </div>
                    
                    <div className="space-y-3 max-h-[600px] overflow-y-auto pr-2">
                        {project.segments.map((seg, idx) => (
                            <div key={seg.id} className="bg-gray-50 p-3 rounded border border-gray-100 text-sm space-y-2 relative group">
                                <div className="flex justify-between items-center border-b border-gray-100 pb-2 mb-2">
                                    <span className="font-mono text-xs text-gray-400 font-bold">SEGMENT #{idx + 1}</span>
                                    {project.segments.length > 1 && (
                                        <button onClick={() => removeSegment(seg.id)} className="text-gray-300 hover:text-red-500 transition-colors">
                                            ×
                                        </button>
                                    )}
                                </div>
                                
                                {/* Definition Mode Row */}
                                <div className="grid grid-cols-2 gap-2 mb-2">
                                    <div>
                                        <label className="text-[10px] text-gray-400 block uppercase">Ref Type</label>
                                        <select 
                                            value={seg.referenceType}
                                            onChange={(e) => updateSegment(seg.id, 'referenceType', e.target.value)}
                                            className="w-full bg-white border border-gray-200 rounded text-xs py-1 px-2"
                                        >
                                            <option value={ReferenceType.MASTER}>Master Pos</option>
                                            <option value={ReferenceType.TIME}>Time Duration</option>
                                        </select>
                                    </div>
                                    <div>
                                        <label className="text-[10px] text-gray-400 block uppercase">Coord Mode</label>
                                        <select 
                                            value={seg.coordinateMode}
                                            onChange={(e) => updateSegment(seg.id, 'coordinateMode', e.target.value)}
                                            className="w-full bg-white border border-gray-200 rounded text-xs py-1 px-2"
                                        >
                                            <option value={CoordinateMode.ABSOLUTE}>Absolute</option>
                                            <option value={CoordinateMode.RELATIVE}>Relative</option>
                                        </select>
                                    </div>
                                </div>

                                {/* Values Row */}
                                <div className="grid grid-cols-2 gap-2">
                                    <div>
                                        <label className="text-[10px] text-gray-400 block uppercase text-indigo-600 font-bold">
                                            {seg.referenceType === ReferenceType.TIME ? 'Duration (s)' : (seg.coordinateMode === CoordinateMode.RELATIVE ? 'Delta Master' : 'Master End')}
                                        </label>
                                        <input 
                                            type="number" 
                                            value={seg.masterVal} 
                                            onChange={(e) => updateSegment(seg.id, 'masterVal', Number(e.target.value))} 
                                            className="w-full bg-white border border-gray-200 rounded px-2 py-1 font-mono text-indigo-700" 
                                        />
                                    </div>
                                    <div>
                                        <label className="text-[10px] text-gray-400 block uppercase text-indigo-600 font-bold">
                                            {seg.coordinateMode === CoordinateMode.RELATIVE ? 'Delta Slave' : 'Slave End'}
                                        </label>
                                        <input 
                                            type="number" 
                                            value={seg.slaveVal} 
                                            onChange={(e) => updateSegment(seg.id, 'slaveVal', Number(e.target.value))} 
                                            className="w-full bg-white border border-gray-200 rounded px-2 py-1 font-mono text-indigo-700" 
                                        />
                                    </div>
                                </div>

                                {/* Law Selection */}
                                <div className="mt-2 pt-2 border-t border-gray-100">
                                     <select 
                                        value={seg.motionLaw}
                                        onChange={(e) => updateSegment(seg.id, 'motionLaw', e.target.value)}
                                        className="w-full bg-transparent text-xs text-gray-500 focus:outline-none"
                                    >
                                        <option value={MotionLawType.POLY5}>Law: Polynomial 5 (R-R)</option>
                                        <option value={MotionLawType.MODIFIED_SINE} disabled>Law: Modified Sine</option>
                                    </select>
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
            <div className="lg:col-span-7 space-y-4">
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
