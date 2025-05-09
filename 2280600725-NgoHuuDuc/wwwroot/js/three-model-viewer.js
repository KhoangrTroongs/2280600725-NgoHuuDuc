/**
 * Three.js Model Viewer
 * A simple and efficient 3D model viewer using Three.js
 */

// Namespace for the model viewer
const ThreeModelViewer = {
    // Store references to created viewers
    viewers: {},

    // Initialize a new viewer
    init: function(containerId, modelUrl, options = {}) {
        console.log(`Initializing viewer for container: ${containerId}, model: ${modelUrl}`);

        // Get container element
        const container = document.getElementById(containerId);
        if (!container) {
            console.error(`Container not found: ${containerId}`);
            return null;
        }

        // Default options
        const defaultOptions = {
            backgroundColor: 0xf8f8f8,
            showGrid: false,
            autoRotate: false,
            cameraPosition: { x: 0, y: 0, z: 5 },
            lights: [
                { type: 'ambient', color: 0xffffff, intensity: 0.7 },
                { type: 'directional', color: 0xffffff, intensity: 1.0, position: { x: 1, y: 1, z: 1 } }
            ]
        };

        // Merge options
        const mergedOptions = Object.assign({}, defaultOptions, options);

        // Create viewer
        try {
            // Show loading message
            container.innerHTML = `
                <div class="text-center p-5">
                    <div class="spinner-border" role="status"></div>
                    <p class="mt-2">Đang tải mô hình 3D...</p>
                </div>
            `;

            // Check if Three.js is loaded
            if (typeof THREE === 'undefined') {
                this._showError(container, 'Thư viện Three.js chưa được tải');
                return null;
            }

            // Check if GLTFLoader is available
            if (typeof THREE.GLTFLoader === 'undefined') {
                this._showError(container, 'GLTFLoader không khả dụng');
                return null;
            }

            // Check if OrbitControls is available
            if (typeof THREE.OrbitControls === 'undefined') {
                this._showError(container, 'OrbitControls không khả dụng');
                return null;
            }

            // Create scene
            const scene = new THREE.Scene();
            scene.background = new THREE.Color(mergedOptions.backgroundColor);

            // Create camera
            const camera = new THREE.PerspectiveCamera(
                75,
                container.clientWidth / container.clientHeight,
                0.1,
                1000
            );
            camera.position.set(
                mergedOptions.cameraPosition.x,
                mergedOptions.cameraPosition.y,
                mergedOptions.cameraPosition.z
            );

            // Create renderer
            const renderer = new THREE.WebGLRenderer({ antialias: true });
            renderer.setSize(container.clientWidth, container.clientHeight);
            renderer.setPixelRatio(window.devicePixelRatio);

            // Clear container and add canvas
            container.innerHTML = '';
            container.appendChild(renderer.domElement);

            // Add lights
            mergedOptions.lights.forEach(function(light) {
                let lightObj;

                if (light.type === 'ambient') {
                    lightObj = new THREE.AmbientLight(light.color, light.intensity);
                } else if (light.type === 'directional') {
                    lightObj = new THREE.DirectionalLight(light.color, light.intensity);
                    lightObj.position.set(light.position.x, light.position.y, light.position.z);
                } else if (light.type === 'point') {
                    lightObj = new THREE.PointLight(light.color, light.intensity);
                    lightObj.position.set(light.position.x, light.position.y, light.position.z);
                }

                if (lightObj) {
                    scene.add(lightObj);
                }
            });

            // Add grid helper if enabled
            if (mergedOptions.showGrid) {
                const gridHelper = new THREE.GridHelper(10, 10);
                scene.add(gridHelper);
            }

            // Add controls
            const controls = new THREE.OrbitControls(camera, renderer.domElement);
            controls.enableDamping = true;
            controls.dampingFactor = 0.05;
            controls.minDistance = 1;
            controls.maxDistance = 20;
            controls.autoRotate = mergedOptions.autoRotate;
            controls.autoRotateSpeed = 1.0;

            // Add instructions
            const instructionsDiv = document.createElement('div');
            instructionsDiv.style.position = 'absolute';
            instructionsDiv.style.bottom = '10px';
            instructionsDiv.style.left = '10px';
            instructionsDiv.style.background = 'rgba(0, 0, 0, 0.5)';
            instructionsDiv.style.color = 'white';
            instructionsDiv.style.padding = '5px 10px';
            instructionsDiv.style.borderRadius = '4px';
            instructionsDiv.style.fontSize = '12px';
            instructionsDiv.style.pointerEvents = 'none';
            instructionsDiv.textContent = 'Kéo để xoay • Cuộn để phóng to/thu nhỏ';
            container.appendChild(instructionsDiv);

            // Create loading indicator
            const loadingDiv = document.createElement('div');
            loadingDiv.style.position = 'absolute';
            loadingDiv.style.top = '50%';
            loadingDiv.style.left = '50%';
            loadingDiv.style.transform = 'translate(-50%, -50%)';
            loadingDiv.style.background = 'rgba(255, 255, 255, 0.8)';
            loadingDiv.style.color = '#333';
            loadingDiv.style.padding = '15px 20px';
            loadingDiv.style.borderRadius = '4px';
            loadingDiv.style.boxShadow = '0 2px 8px rgba(0,0,0,0.2)';
            loadingDiv.style.textAlign = 'center';
            loadingDiv.innerHTML = '<div class="spinner-border spinner-border-sm text-primary" role="status"></div> <span>Đang tải mô hình...</span>';
            container.appendChild(loadingDiv);

            // Handle window resize
            const handleResize = function() {
                camera.aspect = container.clientWidth / container.clientHeight;
                camera.updateProjectionMatrix();
                renderer.setSize(container.clientWidth, container.clientHeight);
            };

            window.addEventListener('resize', handleResize);

            // Animation loop
            const animate = function() {
                requestAnimationFrame(animate);
                controls.update();
                renderer.render(scene, camera);
            };

            // Start animation
            animate();

            // Store viewer reference
            const viewer = {
                container: container,
                scene: scene,
                camera: camera,
                renderer: renderer,
                controls: controls,
                loadingDiv: loadingDiv,
                modelUrl: modelUrl,
                loaded: false,
                dispose: function() {
                    window.removeEventListener('resize', handleResize);
                    controls.dispose();
                    renderer.dispose();
                    container.innerHTML = '';
                    delete ThreeModelViewer.viewers[containerId];
                }
            };

            this.viewers[containerId] = viewer;

            // Load model
            this._loadModel(viewer, modelUrl);

            return viewer;
        } catch (error) {
            console.error('Error initializing viewer:', error);
            this._showError(container, `Lỗi khởi tạo: ${error.message}`);
            return null;
        }
    },

    // Load a 3D model
    _loadModel: function(viewer, modelUrl) {
        try {
            console.log('Loading model:', modelUrl);

            // Create loader
            const loader = new THREE.GLTFLoader();

            // Load the model
            loader.load(
                modelUrl,
                function(gltf) {
                    console.log('Model loaded successfully:', gltf);

                    // Remove loading indicator
                    if (viewer.loadingDiv && viewer.loadingDiv.parentNode) {
                        viewer.loadingDiv.parentNode.removeChild(viewer.loadingDiv);
                    }

                    try {
                        // Center model
                        const box = new THREE.Box3().setFromObject(gltf.scene);
                        const center = new THREE.Vector3();
                        box.getCenter(center);
                        const size = new THREE.Vector3();
                        box.getSize(size);

                        // Reset model position to center
                        gltf.scene.position.x = -center.x;
                        gltf.scene.position.y = -center.y;
                        gltf.scene.position.z = -center.z;

                        // Adjust camera position based on model size
                        const maxDim = Math.max(size.x, size.y, size.z);
                        const fov = viewer.camera.fov * (Math.PI / 180);
                        let cameraDistance = maxDim / (2 * Math.tan(fov / 2));

                        // Add a little extra distance for better view
                        cameraDistance *= 1.5;

                        viewer.camera.position.z = cameraDistance;
                        viewer.controls.update();

                        // Add model to scene
                        viewer.scene.add(gltf.scene);
                        viewer.model = gltf.scene;
                        viewer.loaded = true;

                        console.log('Model added to scene');
                    } catch (error) {
                        console.error('Error processing model:', error);
                        ThreeModelViewer._showError(viewer.container, `Lỗi xử lý mô hình: ${error.message}`);
                    }
                },
                function(xhr) {
                    if (xhr.lengthComputable && viewer.loadingDiv) {
                        const percent = Math.floor((xhr.loaded / xhr.total) * 100);
                        console.log(`Loading model: ${percent}%`);
                        viewer.loadingDiv.innerHTML = `<div class="spinner-border spinner-border-sm text-primary" role="status"></div> <span>Đang tải mô hình: ${percent}%</span>`;
                    }
                },
                function(error) {
                    console.error('Error loading model:', error);

                    // Remove loading indicator
                    if (viewer.loadingDiv && viewer.loadingDiv.parentNode) {
                        viewer.loadingDiv.parentNode.removeChild(viewer.loadingDiv);
                    }

                    ThreeModelViewer._showError(viewer.container, `Lỗi tải mô hình: ${error.message}`);
                }
            );
        } catch (error) {
            console.error('Error in _loadModel:', error);
            this._showError(viewer.container, `Lỗi tải mô hình: ${error.message}`);
        }
    },

    // Show error message
    _showError: function(container, message) {
        console.error('Model viewer error:', message);
        container.innerHTML = `
            <div class="alert alert-danger">
                <h4>Không thể tải mô hình 3D</h4>
                <p>Lỗi: ${message || 'Không xác định'}</p>
                <p>Có thể do một trong các nguyên nhân sau:</p>
                <ul>
                    <li>Định dạng file không được hỗ trợ (chỉ hỗ trợ các file .glb, .gltf)</li>
                    <li>Đường dẫn đến file không chính xác</li>
                    <li>File mô hình bị hỏng</li>
                </ul>
            </div>
        `;
    }
};
