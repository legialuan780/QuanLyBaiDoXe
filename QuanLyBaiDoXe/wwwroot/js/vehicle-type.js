/**
 * Vehicle Type Management Module
 */
const VehicleType = (function () {
    let dataTable = null;

    // API Endpoints
    const API = {
        getVehicleTypes: '/Admin/VehicleType/GetVehicleTypes',
        getVehicleType: '/Admin/VehicleType/GetVehicleType',
        create: '/Admin/VehicleType/Create',
        update: '/Admin/VehicleType/Update',
        delete: '/Admin/VehicleType/Delete',
        getStatistics: '/Admin/VehicleType/GetStatistics'
    };

    /**
     * Initialize the module
     */
    function init() {
        initDataTable();
        bindEvents();
    }

    /**
     * Initialize DataTable
     */
    function initDataTable() {
        dataTable = $('#vehicleTypeTable').DataTable({
            processing: true,
            serverSide: false,
            ajax: {
                url: API.getVehicleTypes,
                dataSrc: 'data'
            },
            columns: [
                {
                    data: null,
                    render: function (data, type, row, meta) {
                        return meta.row + 1;
                    },
                    orderable: false,
                    className: 'text-center'
                },
                {
                    data: 'tenLoaiXe',
                    render: function (data) {
                        return `<span class="vehicle-type-name">${data || '--'}</span>`;
                    }
                },
                {
                    data: 'moTa',
                    render: function (data) {
                        return data || '--';
                    }
                },
                {
                    data: 'soLuongThe',
                    render: function (data) {
                        const badgeClass = data === 0 ? 'zero' : '';
                        return `<span class="count-badge ${badgeClass}">${data}</span>`;
                    },
                    className: 'text-center'
                },
                {
                    data: 'soLuongTheHoatDong',
                    render: function (data) {
                        const badgeClass = data === 0 ? 'zero' : '';
                        return `<span class="count-badge ${badgeClass}">${data}</span>`;
                    },
                    className: 'text-center'
                },
                {
                    data: 'soLuongCauHinhGia',
                    render: function (data) {
                        const badgeClass = data === 0 ? 'zero' : '';
                        return `<span class="count-badge ${badgeClass}">${data}</span>`;
                    },
                    className: 'text-center'
                },
                {
                    data: null,
                    render: function (data) {
                        return `
                            <div class="action-buttons">
                                <button class="btn-action btn-action-view" onclick="VehicleType.showDetail(${data.maLoaiXe})" title="Xem chi tiết">
                                    <i class="fas fa-eye"></i>
                                </button>
                                <button class="btn-action btn-action-edit" onclick="VehicleType.openEditModal(${data.maLoaiXe})" title="Chỉnh sửa">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn-action btn-action-delete" onclick="VehicleType.confirmDelete(${data.maLoaiXe}, '${data.tenLoaiXe}')" title="Xóa">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        `;
                    },
                    orderable: false
                }
            ],
            order: [[1, 'asc']], // Sort by name ascending
            language: {
                processing: "Đang xử lý...",
                search: "Tìm kiếm:",
                lengthMenu: "Hiển thị _MENU_ dòng",
                info: "Đang xem _START_ đến _END_ trong tổng số _TOTAL_ loại xe",
                infoEmpty: "Không có dữ liệu",
                infoFiltered: "(được lọc từ _MAX_ dòng)",
                loadingRecords: "Đang tải...",
                zeroRecords: "Không tìm thấy kết quả phù hợp",
                emptyTable: "Không có dữ liệu",
                paginate: {
                    first: "Đầu",
                    previous: "Trước",
                    next: "Tiếp",
                    last: "Cuối"
                }
            },
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Tất cả"]],
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip'
        });
    }

    /**
     * Bind form and button events
     */
    function bindEvents() {
        // Add form submit
        $('#addForm').on('submit', function (e) {
            e.preventDefault();
            handleCreate();
        });

        // Edit form submit
        $('#editForm').on('submit', function (e) {
            e.preventDefault();
            handleUpdate();
        });

        // Close modal on overlay click
        $('.modal-overlay').on('click', function (e) {
            if ($(e.target).hasClass('modal-overlay')) {
                closeModal(this.id);
            }
        });

        // Close modal on Escape key
        $(document).on('keydown', function (e) {
            if (e.key === 'Escape') {
                $('.modal-overlay.show').each(function () {
                    closeModal(this.id);
                });
            }
        });
    }

    /**
     * Open add modal
     */
    function openAddModal() {
        $('#addForm')[0].reset();
        openModal('addModal');
    }

    /**
     * Open edit modal
     */
    function openEditModal(id) {
        showLoading();

        $.ajax({
            url: `${API.getVehicleType}?id=${id}`,
            method: 'GET',
            success: function (response) {
                hideLoading();

                if (response.success) {
                    const data = response.data;
                    $('#editMaLoaiXe').val(data.maLoaiXe);
                    $('#editTenLoaiXe').val(data.tenLoaiXe);
                    $('#editMoTa').val(data.moTa || '');
                    openModal('editModal');
                } else {
                    showNotification(response.message || 'Không thể tải thông tin loại xe', 'error');
                }
            },
            error: function () {
                hideLoading();
                showNotification('Đã xảy ra lỗi khi tải thông tin loại xe', 'error');
            }
        });
    }

    /**
     * Show detail modal
     */
    function showDetail(id) {
        showLoading();

        $.ajax({
            url: `${API.getVehicleType}?id=${id}`,
            method: 'GET',
            success: function (response) {
                hideLoading();

                if (response.success) {
                    const data = response.data;
                    $('#detailMaLoaiXe').text(data.maLoaiXe);
                    $('#detailTenLoaiXe').text(data.tenLoaiXe || '--');
                    $('#detailMoTa').text(data.moTa || '--');
                    $('#detailSoLuongThe').text(data.soLuongThe);
                    $('#detailSoLuongTheHoatDong').text(data.soLuongTheHoatDong);
                    $('#detailSoLuongCauHinhGia').text(data.soLuongCauHinhGia);
                    openModal('detailModal');
                } else {
                    showNotification(response.message || 'Không thể tải thông tin loại xe', 'error');
                }
            },
            error: function () {
                hideLoading();
                showNotification('Đã xảy ra lỗi khi tải thông tin loại xe', 'error');
            }
        });
    }

    /**
     * Handle create vehicle type
     */
    function handleCreate() {
        const formData = {
            tenLoaiXe: $('#addTenLoaiXe').val().trim(),
            moTa: $('#addMoTa').val().trim() || null
        };

        if (!formData.tenLoaiXe) {
            showNotification('Vui lòng nhập tên loại xe', 'error');
            return;
        }

        showLoading();

        $.ajax({
            url: API.create,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                hideLoading();

                if (response.success) {
                    showNotification(response.message, 'success');
                    closeModal('addModal');
                    reloadData();
                } else {
                    showNotification(response.message, 'error');
                }
            },
            error: function () {
                hideLoading();
                showNotification('Đã xảy ra lỗi khi thêm loại xe', 'error');
            }
        });
    }

    /**
     * Handle update vehicle type
     */
    function handleUpdate() {
        const formData = {
            maLoaiXe: parseInt($('#editMaLoaiXe').val()),
            tenLoaiXe: $('#editTenLoaiXe').val().trim(),
            moTa: $('#editMoTa').val().trim() || null
        };

        if (!formData.tenLoaiXe) {
            showNotification('Vui lòng nhập tên loại xe', 'error');
            return;
        }

        showLoading();

        $.ajax({
            url: API.update,
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                hideLoading();

                if (response.success) {
                    showNotification(response.message, 'success');
                    closeModal('editModal');
                    reloadData();
                } else {
                    showNotification(response.message, 'error');
                }
            },
            error: function () {
                hideLoading();
                showNotification('Đã xảy ra lỗi khi cập nhật loại xe', 'error');
            }
        });
    }

    /**
     * Confirm delete vehicle type
     */
    function confirmDelete(id, name) {
        Swal.fire({
            title: 'Xác nhận xóa',
            html: `Bạn có chắc chắn muốn xóa loại xe <strong>${name}</strong>?<br><small class="text-muted">Lưu ý: Không thể xóa nếu loại xe đang có thẻ hoặc cấu hình giá liên kết.</small>`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#e74c3c',
            cancelButtonColor: '#95a5a6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                handleDelete(id);
            }
        });
    }

    /**
     * Handle delete vehicle type
     */
    function handleDelete(id) {
        showLoading();

        $.ajax({
            url: `${API.delete}?id=${id}`,
            method: 'DELETE',
            success: function (response) {
                hideLoading();

                if (response.success) {
                    showNotification(response.message, 'success');
                    reloadData();
                } else {
                    Swal.fire({
                        title: 'Không thể xóa',
                        text: response.message,
                        icon: 'error',
                        confirmButtonColor: '#3498db'
                    });
                }
            },
            error: function () {
                hideLoading();
                showNotification('Đã xảy ra lỗi khi xóa loại xe', 'error');
            }
        });
    }

    /**
     * Reload DataTable and statistics
     */
    function reloadData() {
        dataTable.ajax.reload(null, false);
        updateStatistics();
    }

    /**
     * Update statistics
     */
    function updateStatistics() {
        $.ajax({
            url: API.getStatistics,
            method: 'GET',
            success: function (data) {
                $('#statTotalTypes').text(data.totalVehicleTypes);
                $('#statTotalCards').text(data.totalCards);
                $('#statActiveCards').text(data.activeCards);
            },
            error: function () {
                console.error('Failed to load statistics');
            }
        });
    }

    /**
     * Open modal
     */
    function openModal(modalId) {
        $(`#${modalId}`).addClass('show');
        $('body').css('overflow', 'hidden');
    }

    /**
     * Close modal
     */
    function closeModal(modalId) {
        $(`#${modalId}`).removeClass('show');
        $('body').css('overflow', 'auto');
    }

    /**
     * Show loading overlay
     */
    function showLoading() {
        $('#loadingOverlay').addClass('show');
    }

    /**
     * Hide loading overlay
     */
    function hideLoading() {
        $('#loadingOverlay').removeClass('show');
    }

    /**
     * Show notification
     */
    function showNotification(message, type = 'success') {
        const notification = $('#notification');
        notification.removeClass('success error info').addClass(type).text(message).addClass('show');

        setTimeout(function () {
            notification.removeClass('show');
        }, 3000);
    }

    // Initialize on document ready
    $(document).ready(function () {
        init();
    });

    // Public API
    return {
        openAddModal: openAddModal,
        openEditModal: openEditModal,
        showDetail: showDetail,
        confirmDelete: confirmDelete,
        closeModal: closeModal,
        reloadData: reloadData
    };
})();
