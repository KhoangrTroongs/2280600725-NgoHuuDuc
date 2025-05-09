import React from 'react';

const ModelViewerFallback = ({ width = '100%', height = '400px' }) => {
  return (
    <div 
      style={{ 
        width, 
        height, 
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        backgroundColor: '#f8f8f8',
        borderRadius: '4px',
        boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
        padding: '20px',
        textAlign: 'center'
      }}
    >
      <div style={{ fontSize: '48px', marginBottom: '20px' }}>
        <i className="fas fa-cube"></i>
      </div>
      <h4>3D Preview Not Available</h4>
      <p>
        The 3D model viewer could not be loaded. This could be due to:
      </p>
      <ul style={{ textAlign: 'left' }}>
        <li>Missing 3D model file</li>
        <li>WebGL not supported by your browser</li>
        <li>Required libraries not loaded</li>
      </ul>
      <p>
        Please try a different browser or device.
      </p>
    </div>
  );
};

export default ModelViewerFallback;
