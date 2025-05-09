/**
 * Simple Model Viewer
 * A basic 3D model viewer that displays a 3D model using Three.js
 */

// Function to create a model viewer
function createModelViewer(containerId, modelUrl) {
    // Get the container element
    const container = document.getElementById(containerId);
    if (!container) {
        console.error('Container element not found:', containerId);
        return false;
    }

    try {
        // Check if model URL is valid
        if (!modelUrl || modelUrl.trim() === '') {
            console.error('Model URL is empty');
            container.innerHTML = '<div class="alert alert-danger">Không tìm thấy đường dẫn đến mô hình 3D</div>';
            return false;
        }

        // Log the model URL for debugging
        console.log('Attempting to load 3D model from URL:', modelUrl);

        // Hiển thị thông báo đang tải
        container.innerHTML = `
            <div class="text-center p-5">
                <div class="spinner-border" role="status"></div>
                <p class="mt-2">Đang tải mô hình 3D...</p>
            </div>
        `;

        // Kiểm tra xem file có tồn tại không
        fetch(modelUrl)
            .then(response => {
                console.log('Fetch response status:', response.status);
                if (!response.ok) {
                    showError(container, modelUrl, `Không thể truy cập file (HTTP ${response.status})`);
                    return;
                }

                // Hiển thị mô hình 3D hoặc hình ảnh dựa vào định dạng file
                const fileExtension = modelUrl.split('.').pop().toLowerCase();
                console.log('File extension:', fileExtension);

                if (fileExtension === 'glb' || fileExtension === 'gltf') {
                    // Tải và hiển thị mô hình 3D
                    initThreeJSViewer(container, modelUrl);
                } else {
                    // Hiển thị thông báo định dạng không được hỗ trợ
                    showError(container, modelUrl, `Định dạng file không được hỗ trợ: ${fileExtension}`);
                }
            })
            .catch(error => {
                console.error('Error checking model URL:', error);
                showError(container, modelUrl, error.message);
            });

        return true;
    } catch (error) {
        console.error('Error creating model viewer:', error);
        showError(container, modelUrl, error.message);
        return false;
    }
}

// Khởi tạo trình xem 3D sử dụng Three.js
function initThreeJSViewer(container, modelUrl) {
    console.log('Initializing Three.js viewer for model:', modelUrl);

    try {
        // Kiểm tra xem Three.js đã được tải chưa
        if (typeof THREE === 'undefined') {
            console.error('Three.js is not loaded');
            showError(container, modelUrl, 'Thư viện Three.js chưa được tải. Vui lòng làm mới trang và thử lại.');
            return;
        }

        // Kiểm tra xem GLTFLoader đã được tải chưa
        if (!THREE.GLTFLoader) {
            console.error('THREE.GLTFLoader is not defined');
            showError(container, modelUrl, 'Module GLTFLoader chưa được tải. Vui lòng làm mới trang và thử lại.');
            return;
        }

        // Kiểm tra xem OrbitControls đã được tải chưa
        if (!THREE.OrbitControls) {
            console.error('THREE.OrbitControls is not defined');
            showError(container, modelUrl, 'Module OrbitControls chưa được tải. Vui lòng làm mới trang và thử lại.');
            return;
        }

        // Tạo scene
        const scene = new THREE.Scene();
        scene.background = new THREE.Color(0xf8f8f8);

        // Tạo camera
        const camera = new THREE.PerspectiveCamera(
            75,
            container.clientWidth / container.clientHeight,
            0.1,
            1000
        );
        camera.position.z = 5;

        // Tạo renderer
        const renderer = new THREE.WebGLRenderer({ antialias: true });
        renderer.setSize(container.clientWidth, container.clientHeight);
        renderer.setPixelRatio(window.devicePixelRatio);

        // Xử lý sự kiện thay đổi kích thước cửa sổ
        const handleResize = () => {
            const width = container.clientWidth;
            const height = container.clientHeight;

            camera.aspect = width / height;
            camera.updateProjectionMatrix();

            renderer.setSize(width, height);
        };

        window.addEventListener('resize', handleResize);

        // Xóa nội dung hiện tại và thêm canvas
        container.innerHTML = '';
        container.appendChild(renderer.domElement);

        // Thêm ánh sáng
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.7);
        scene.add(ambientLight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 1);
        directionalLight.position.set(1, 1, 1);
        scene.add(directionalLight);

        // Thêm controls
        const controls = new THREE.OrbitControls(camera, renderer.domElement);
        controls.enableDamping = true;
        controls.dampingFactor = 0.05;
        controls.minDistance = 1;
        controls.maxDistance = 20;

        // Hiển thị thông tin hướng dẫn
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

        // Tạo loading indicator
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

        // Tải mô hình
        console.log('Creating GLTFLoader instance');
        const loader = new THREE.GLTFLoader();

        console.log('Starting to load model from URL:', modelUrl);
        loader.load(
            modelUrl,
            (gltf) => {
                console.log('Model loaded successfully:', gltf);

                // Xóa loading indicator
                if (loadingDiv.parentNode) {
                    loadingDiv.parentNode.removeChild(loadingDiv);
                }

                try {
                    // Căn giữa mô hình
                    const box = new THREE.Box3().setFromObject(gltf.scene);
                    const center = box.getCenter(new THREE.Vector3());
                    const size = box.getSize(new THREE.Vector3());

                    // Đặt lại vị trí mô hình về trung tâm
                    gltf.scene.position.x = -center.x;
                    gltf.scene.position.y = -center.y;
                    gltf.scene.position.z = -center.z;

                    // Điều chỉnh vị trí camera dựa trên kích thước mô hình
                    const maxDim = Math.max(size.x, size.y, size.z);
                    const fov = camera.fov * (Math.PI / 180);
                    let cameraDistance = maxDim / (2 * Math.tan(fov / 2));

                    // Thêm một chút khoảng cách để có góc nhìn tốt hơn
                    cameraDistance *= 1.5;

                    camera.position.z = cameraDistance;
                    controls.update();

                    // Thêm mô hình vào scene
                    scene.add(gltf.scene);

                    console.log('Model added to scene');
                } catch (error) {
                    console.error('Error processing loaded model:', error);
                    showError(container, modelUrl, `Lỗi khi xử lý mô hình: ${error.message}`);
                }
            },
            (xhr) => {
                // Hiển thị tiến trình tải
                if (xhr.lengthComputable) {
                    const percent = Math.floor((xhr.loaded / xhr.total) * 100);
                    console.log(`Đang tải mô hình: ${percent}%`);
                    loadingDiv.innerHTML = `<div class="spinner-border spinner-border-sm text-primary" role="status"></div> <span>Đang tải mô hình: ${percent}%</span>`;
                }
            },
            (error) => {
                console.error('Error loading model:', error);

                // Xóa loading indicator
                if (loadingDiv.parentNode) {
                    loadingDiv.parentNode.removeChild(loadingDiv);
                }

                showError(container, modelUrl, `Lỗi khi tải mô hình: ${error.message}`);
            }
        );

        // Animation loop
        function animate() {
            requestAnimationFrame(animate);
            controls.update();
            renderer.render(scene, camera);
        }

        animate();

        console.log('Three.js viewer initialized successfully');

    } catch (error) {
        console.error('Error in initThreeJSViewer:', error);
        showError(container, modelUrl, `Lỗi khởi tạo trình xem 3D: ${error.message}`);
    }
}

// Hiển thị thông báo lỗi
function showError(container, modelUrl, errorMessage) {
    console.error('Model viewer error:', errorMessage, 'URL:', modelUrl);
    container.innerHTML = `
        <div class="alert alert-danger">
            <h4>Không thể tải mô hình 3D</h4>
            <p>Lỗi: ${errorMessage || 'Không xác định'}</p>
            <p>Có thể do một trong các nguyên nhân sau:</p>
            <ul>
                <li>Định dạng file không được hỗ trợ (chỉ hỗ trợ các file .glb, .gltf)</li>
                <li>Đường dẫn đến file không chính xác</li>
                <li>File mô hình bị hỏng</li>
            </ul>
            <p>URL: ${modelUrl}</p>
            <p><a href="${modelUrl}" target="_blank">Kiểm tra đường dẫn</a></p>
        </div>
    `;
}
