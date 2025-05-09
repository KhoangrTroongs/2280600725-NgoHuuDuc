/**
 * Three.js Model Viewer
 * Utility functions for loading and displaying 3D models
 */

// Check if Three.js is loaded
if (typeof THREE === 'undefined') {
    console.error('Three.js is not loaded. Please include the Three.js library before this script.');
}

// Global object to store viewer functions
const ThreeViewer = {
    // Initialize the viewer with a container element and model URL
    init: function(containerId, modelUrl) {
        if (!modelUrl) {
            console.error('No model URL provided');
            return false;
        }
        
        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Container element not found:', containerId);
            return false;
        }
        
        try {
            // Create scene
            const scene = new THREE.Scene();
            scene.background = new THREE.Color(0xf8f8f8);
            
            // Create camera
            const camera = new THREE.PerspectiveCamera(
                75,
                container.clientWidth / container.clientHeight,
                0.1,
                1000
            );
            camera.position.z = 5;
            
            // Create renderer
            const renderer = new THREE.WebGLRenderer({ antialias: true });
            renderer.setSize(container.clientWidth, container.clientHeight);
            renderer.setPixelRatio(window.devicePixelRatio);
            container.appendChild(renderer.domElement);
            
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
            
            // Add grid helper for reference
            const gridHelper = new THREE.GridHelper(10, 10);
            scene.add(gridHelper);
            
            // Load model based on file extension
            const fileExtension = modelUrl.split('.').pop().toLowerCase();
            
            // Show loading indicator
            container.innerHTML = '<div class="text-center p-5"><div class="spinner-border" role="status"></div><p class="mt-2">Đang tải mô hình 3D...</p></div>';
            
            // Load model based on file extension
            if (fileExtension === 'glb' || fileExtension === 'gltf') {
                if (typeof THREE.GLTFLoader === 'undefined') {
                    container.innerHTML = '<div class="alert alert-danger">GLTFLoader không khả dụng</div>';
                    return false;
                }
                
                const loader = new THREE.GLTFLoader();
                loader.load(
                    modelUrl,
                    function(gltf) {
                        // Clear loading indicator
                        container.innerHTML = '';
                        container.appendChild(renderer.domElement);
                        
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
                        
                        // Start animation loop
                        animate();
                    },
                    function(xhr) {
                        const percent = Math.round((xhr.loaded / xhr.total) * 100);
                        console.log(`${percent}% loaded`);
                    },
                    function(error) {
                        console.error('Error loading GLTF/GLB model:', error);
                        container.innerHTML = `<div class="alert alert-danger">
                            <p>Không thể tải mô hình 3D. Lỗi: ${error.message || 'Không xác định'}</p>
                            <p>URL: ${modelUrl}</p>
                        </div>`;
                    }
                );
            } else if (fileExtension === 'obj') {
                if (typeof THREE.OBJLoader === 'undefined') {
                    container.innerHTML = '<div class="alert alert-danger">OBJLoader không khả dụng</div>';
                    return false;
                }
                
                const loader = new THREE.OBJLoader();
                loader.load(
                    modelUrl,
                    function(object) {
                        // Clear loading indicator
                        container.innerHTML = '';
                        container.appendChild(renderer.domElement);
                        
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
                        
                        // Start animation loop
                        animate();
                    },
                    function(xhr) {
                        const percent = Math.round((xhr.loaded / xhr.total) * 100);
                        console.log(`${percent}% loaded`);
                    },
                    function(error) {
                        console.error('Error loading OBJ model:', error);
                        container.innerHTML = `<div class="alert alert-danger">
                            <p>Không thể tải mô hình 3D. Lỗi: ${error.message || 'Không xác định'}</p>
                            <p>URL: ${modelUrl}</p>
                        </div>`;
                    }
                );
            } else {
                container.innerHTML = `<div class="alert alert-warning">
                    Định dạng file không được hỗ trợ: ${fileExtension}. 
                    Chỉ hỗ trợ các định dạng: .glb, .gltf, .obj
                </div>`;
                return false;
            }
            
            // Handle window resize
            function handleResize() {
                camera.aspect = container.clientWidth / container.clientHeight;
                camera.updateProjectionMatrix();
                renderer.setSize(container.clientWidth, container.clientHeight);
            }
            
            window.addEventListener('resize', handleResize);
            
            // Animation loop
            function animate() {
                requestAnimationFrame(animate);
                controls.update();
                renderer.render(scene, camera);
            }
            
            return true;
        } catch (error) {
            console.error('Error initializing Three.js viewer:', error);
            container.innerHTML = `<div class="alert alert-danger">
                <p>Lỗi khi khởi tạo trình xem 3D: ${error.message || 'Không xác định'}</p>
            </div>`;
            return false;
        }
    }
};
