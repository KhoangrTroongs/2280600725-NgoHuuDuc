import React, { useRef, useEffect } from 'react';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader';

const ModelViewer = ({ modelPath, width = '100%', height = '400px' }) => {
  const mountRef = useRef(null);
  const sceneRef = useRef(null);
  const cameraRef = useRef(null);
  const rendererRef = useRef(null);
  const controlsRef = useRef(null);
  const animationFrameRef = useRef(null);

  useEffect(() => {
    // Initialize scene, camera, and renderer
    const initThree = () => {
      if (!mountRef.current) return;

      // Create scene
      const scene = new THREE.Scene();
      scene.background = new THREE.Color(0xf8f8f8);
      sceneRef.current = scene;

      // Create camera
      const camera = new THREE.PerspectiveCamera(
        75,
        mountRef.current.clientWidth / mountRef.current.clientHeight,
        0.1,
        1000
      );
      camera.position.z = 5;
      cameraRef.current = camera;

      // Create renderer
      const renderer = new THREE.WebGLRenderer({ antialias: true });
      renderer.setSize(mountRef.current.clientWidth, mountRef.current.clientHeight);
      renderer.setPixelRatio(window.devicePixelRatio);
      renderer.outputEncoding = THREE.sRGBEncoding;
      mountRef.current.appendChild(renderer.domElement);
      rendererRef.current = renderer;

      // Add lights
      const ambientLight = new THREE.AmbientLight(0xffffff, 0.5);
      scene.add(ambientLight);

      const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
      directionalLight.position.set(1, 1, 1);
      scene.add(directionalLight);

      // Add controls
      const controls = new OrbitControls(camera, renderer.domElement);
      controls.enableDamping = true;
      controls.dampingFactor = 0.05;
      controls.minDistance = 3;
      controls.maxDistance = 10;
      controlsRef.current = controls;

      // Add grid helper for reference
      const gridHelper = new THREE.GridHelper(10, 10);
      scene.add(gridHelper);

      // Handle window resize
      const handleResize = () => {
        if (!mountRef.current || !cameraRef.current || !rendererRef.current) return;
        
        const width = mountRef.current.clientWidth;
        const height = mountRef.current.clientHeight;
        
        cameraRef.current.aspect = width / height;
        cameraRef.current.updateProjectionMatrix();
        
        rendererRef.current.setSize(width, height);
      };

      window.addEventListener('resize', handleResize);

      return () => {
        window.removeEventListener('resize', handleResize);
      };
    };

    // Load 3D model
    const loadModel = () => {
      if (!sceneRef.current || !modelPath) return;

      const loader = new GLTFLoader();
      
      // Show loading indicator
      const loadingManager = new THREE.LoadingManager();
      loadingManager.onProgress = (url, loaded, total) => {
        console.log(`Loading model: ${Math.round((loaded / total) * 100)}%`);
      };

      loader.setPath('');
      loader.load(
        modelPath,
        (gltf) => {
          // Center model
          const box = new THREE.Box3().setFromObject(gltf.scene);
          const center = box.getCenter(new THREE.Vector3());
          const size = box.getSize(new THREE.Vector3());
          
          // Reset model position to center
          gltf.scene.position.x = -center.x;
          gltf.scene.position.y = -center.y;
          gltf.scene.position.z = -center.z;
          
          // Adjust camera position based on model size
          const maxDim = Math.max(size.x, size.y, size.z);
          const fov = cameraRef.current.fov * (Math.PI / 180);
          let cameraDistance = maxDim / (2 * Math.tan(fov / 2));
          
          // Add a little extra distance for better view
          cameraDistance *= 1.5;
          
          cameraRef.current.position.z = cameraDistance;
          
          // Reset controls
          controlsRef.current.target.set(0, 0, 0);
          controlsRef.current.update();
          
          // Add model to scene
          sceneRef.current.add(gltf.scene);
        },
        (xhr) => {
          console.log(`${(xhr.loaded / xhr.total) * 100}% loaded`);
        },
        (error) => {
          console.error('Error loading model:', error);
        }
      );
    };

    // Animation loop
    const animate = () => {
      animationFrameRef.current = requestAnimationFrame(animate);
      
      if (controlsRef.current) {
        controlsRef.current.update();
      }
      
      if (rendererRef.current && sceneRef.current && cameraRef.current) {
        rendererRef.current.render(sceneRef.current, cameraRef.current);
      }
    };

    // Initialize Three.js
    const cleanupFn = initThree();
    
    // Load model
    loadModel();
    
    // Start animation loop
    animate();

    // Cleanup function
    return () => {
      if (cleanupFn) cleanupFn();
      
      if (animationFrameRef.current) {
        cancelAnimationFrame(animationFrameRef.current);
      }
      
      if (mountRef.current && rendererRef.current) {
        mountRef.current.removeChild(rendererRef.current.domElement);
      }
      
      if (sceneRef.current) {
        sceneRef.current.clear();
      }
    };
  }, [modelPath]);

  return (
    <div 
      ref={mountRef} 
      style={{ 
        width, 
        height, 
        position: 'relative',
        overflow: 'hidden',
        borderRadius: '4px',
        boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)'
      }}
    >
      <div style={{
        position: 'absolute',
        bottom: '10px',
        left: '10px',
        background: 'rgba(0, 0, 0, 0.5)',
        color: 'white',
        padding: '5px 10px',
        borderRadius: '4px',
        fontSize: '12px'
      }}>
        Drag to rotate • Scroll to zoom
      </div>
    </div>
  );
};

export default ModelViewer;
