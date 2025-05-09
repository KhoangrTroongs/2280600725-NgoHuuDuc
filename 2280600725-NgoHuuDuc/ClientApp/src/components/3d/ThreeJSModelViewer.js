import React, { useRef, useEffect } from 'react';
import * as THREE from 'three';
import { OrbitControls } from 'three/examples/jsm/controls/OrbitControls';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader';
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader';

const ThreeJSModelViewer = ({ modelUrl, width = '100%', height = '400px' }) => {
  const containerRef = useRef(null);
  const rendererRef = useRef(null);
  const sceneRef = useRef(null);
  const cameraRef = useRef(null);
  const controlsRef = useRef(null);
  const frameIdRef = useRef(null);

  useEffect(() => {
    if (!containerRef.current || !modelUrl) return;

    // Initialize Three.js scene
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0xf8f8f8);
    sceneRef.current = scene;

    // Set up camera
    const camera = new THREE.PerspectiveCamera(
      75,
      containerRef.current.clientWidth / containerRef.current.clientHeight,
      0.1,
      1000
    );
    camera.position.z = 5;
    cameraRef.current = camera;

    // Set up renderer
    const renderer = new THREE.WebGLRenderer({ antialias: true });
    renderer.setSize(containerRef.current.clientWidth, containerRef.current.clientHeight);
    renderer.setPixelRatio(window.devicePixelRatio);
    containerRef.current.appendChild(renderer.domElement);
    rendererRef.current = renderer;

    // Add lights
    const ambientLight = new THREE.AmbientLight(0xffffff, 0.5);
    scene.add(ambientLight);

    const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
    directionalLight.position.set(1, 1, 1);
    scene.add(directionalLight);

    // Add controls
    const controls = new THREE.OrbitControls(camera, renderer.domElement);
    controls.enableDamping = true;
    controls.dampingFactor = 0.05;
    controls.minDistance = 3;
    controls.maxDistance = 10;
    controlsRef.current = controls;

    // Add grid helper for reference
    const gridHelper = new THREE.GridHelper(10, 10);
    scene.add(gridHelper);

    // Load model based on file extension
    const fileExtension = modelUrl.split('.').pop().toLowerCase();
    
    if (fileExtension === 'glb' || fileExtension === 'gltf') {
      const loader = new GLTFLoader();
      loader.load(
        modelUrl,
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
          const fov = camera.fov * (Math.PI / 180);
          let cameraDistance = maxDim / (2 * Math.tan(fov / 2));
          
          // Add a little extra distance for better view
          cameraDistance *= 1.5;
          
          camera.position.z = cameraDistance;
          
          // Reset controls
          controls.target.set(0, 0, 0);
          controls.update();
          
          // Add model to scene
          scene.add(gltf.scene);
        },
        (xhr) => {
          console.log(`${(xhr.loaded / xhr.total) * 100}% loaded`);
        },
        (error) => {
          console.error('Error loading model:', error);
        }
      );
    } else if (fileExtension === 'obj') {
      const loader = new OBJLoader();
      loader.load(
        modelUrl,
        (object) => {
          // Center model
          const box = new THREE.Box3().setFromObject(object);
          const center = box.getCenter(new THREE.Vector3());
          const size = box.getSize(new THREE.Vector3());
          
          // Reset model position to center
          object.position.x = -center.x;
          object.position.y = -center.y;
          object.position.z = -center.z;
          
          // Adjust camera position based on model size
          const maxDim = Math.max(size.x, size.y, size.z);
          const fov = camera.fov * (Math.PI / 180);
          let cameraDistance = maxDim / (2 * Math.tan(fov / 2));
          
          // Add a little extra distance for better view
          cameraDistance *= 1.5;
          
          camera.position.z = cameraDistance;
          
          // Reset controls
          controls.target.set(0, 0, 0);
          controls.update();
          
          // Add model to scene
          scene.add(object);
        },
        (xhr) => {
          console.log(`${(xhr.loaded / xhr.total) * 100}% loaded`);
        },
        (error) => {
          console.error('Error loading model:', error);
        }
      );
    }

    // Animation loop
    const animate = () => {
      frameIdRef.current = requestAnimationFrame(animate);
      
      if (controlsRef.current) {
        controlsRef.current.update();
      }
      
      if (rendererRef.current && sceneRef.current && cameraRef.current) {
        rendererRef.current.render(sceneRef.current, cameraRef.current);
      }
    };
    
    animate();

    // Handle window resize
    const handleResize = () => {
      if (!containerRef.current || !cameraRef.current || !rendererRef.current) return;
      
      const width = containerRef.current.clientWidth;
      const height = containerRef.current.clientHeight;
      
      cameraRef.current.aspect = width / height;
      cameraRef.current.updateProjectionMatrix();
      
      rendererRef.current.setSize(width, height);
    };
    
    window.addEventListener('resize', handleResize);

    // Cleanup
    return () => {
      window.removeEventListener('resize', handleResize);
      
      if (frameIdRef.current) {
        cancelAnimationFrame(frameIdRef.current);
      }
      
      if (containerRef.current && rendererRef.current) {
        containerRef.current.removeChild(rendererRef.current.domElement);
      }
      
      if (controlsRef.current) {
        controlsRef.current.dispose();
      }
      
      if (rendererRef.current) {
        rendererRef.current.dispose();
      }
    };
  }, [modelUrl]);

  return (
    <div 
      ref={containerRef} 
      style={{ 
        width, 
        height, 
        position: 'relative',
        borderRadius: '4px',
        overflow: 'hidden',
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
        fontSize: '12px',
        pointerEvents: 'none'
      }}>
        Kéo để xoay • Cuộn để phóng to/thu nhỏ
      </div>
    </div>
  );
};

export default ThreeJSModelViewer;
