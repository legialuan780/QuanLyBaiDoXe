/**
 * Vehicle History Management Module
 */
const VehicleHistory = (function () {
    let dataTable = null;

    // API Endpoints
    const API = {
        getVehicles: '/Admin/VehicleHistory/GetVehicles',
        getDetail: '/Admin/VehicleHistory/GetVehicleDetail',
        getStatistics: '/Admin/VehicleHistory/GetStatistics'
    };

    /**
     * Initialize the module
     */
    function init() {
        initDataTable();
        bindEvents();
        setDefaultDates();
    }

    /**
     * Set default date filters (last 30 days)
     */
    function setDefaultDates() {
        const today = new Date();
        const thirtyDaysAgo = new Date(today);
        thirtyDaysAgo.setDate(today.getDate() - 30);

        $('#filterFromDate').val(formatDateForInput(thirtyDaysAgo));
        $('#filterToDate').val(formatDateForInput(today));
    }

    /**
     * Format date for input[type="date"]
     */
    function formatDateForInput(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    }

    /**
     * Initialize DataTable
     */
    function initDataTable() {
        dataTable = $('#vehicleHistoryTable').DataTable({
            processing: true,
            serverSide: false,
            ajax: {
                url: API.getVehicles,
                data: function (d) {
                    return getFilterParams();
                },
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
                    data: 'bienSoVao',
                    render: function (data) {
                        return data ? `<span class="license-plate">${data}</span>` : '--';
                    }
                },
                {
                    data: 'maThe',
                    render: function (data) {
                        return data ? `<span class="card-code">${data}</span>` : '--';
                    }
                },
                {
                    data: 'tenLoaiXe',
                    render: function (data) {
                        return data || '--';
                    }
                },
                {
                    data: null,
                    render: function (data) {
                        const viTri = data.tenViTri || '--';
                        const khuVuc = data.tenKhuVuc || '';
                        return khuVuc ? `${viTri} (${khuVuc})` : viTri;
                    }
                },
                {
                    data: 'thoiGianVao',
                    render: function (data) {
                        return data ? formatDateTime(data) : '--';
                    }
                },
                {
                    data: 'thoiGianRa',
                    render: function (data) {
                        return data ? formatDateTime(data) : '--';
                    }
                },
                {
                    data: 'thoiGianGuiFormatted',
                    render: function (data) {
                        return `<span class="time-duration">${data}</span>`;
                    }
                },
                {
                    data: 'tongTien',
                    render: function (data) {
                        if (data === null || data === undefined) return '--';
                        return `<span class="price">${formatCurrency(data)}</span>`;
                    }
                },
                {
                    data: 'trangThai',
                    render: function (data) {
                        if (data === 0) {
                            return '<span class="status-badge in-progress">Đang gửi</span>';
                        } else {
                            return '<span class="status-badge completed">Đã lấy xe</span>';
                        }
                    }
                }
            ],
            order: [[5, 'desc']], // Sort by entry time descending
            language: {
                processing: "Đang xử lý...",
                search: "Tìm kiếm:",
                lengthMenu: "Hiển thị _MENU_ dòng",
                info: "Đang xem _START_ đến _END_ trong tổng số _TOTAL_ lượt gửi",
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
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip'
        });

        // Row click event
        $('#vehicleHistoryTable tbody').on('click', 'tr', function () {
            const data = dataTable.row(this).data();
            if (data) {
                showVehicleDetail(data.maLuotGui);
            }
        });
    }

    /**
     * Bind form and button events
     */
    function bindEvents() {
        // Filter form submit
        $('#filterForm').on('submit', function (e) {
            e.preventDefault();
            reloadData();
        });

        // Reset filter
        $('#btnResetFilter').on('click', function () {
            $('#filterForm')[0].reset();
            setDefaultDates();
            reloadData();
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
     * Get filter parameters
     */
    function getFilterParams() {
        return {
            fromDate: $('#filterFromDate').val() || null,
            toDate: $('#filterToDate').val() || null,
            keyword: $('#filterKeyword').val() || null,
            loaiXe: $('#filterLoaiXe').val() || null,
            trangThai: $('#filterTrangThai').val() || null
        };
    }

    /**
     * Reload DataTable and statistics
     */
    function reloadData() {
        showLoading();

        // Reload table
        dataTable.ajax.reload(function () {
            hideLoading();
        });

        // Reload statistics
        updateStatistics();
    }

    /**
     * Update statistics based on current filters
     */
    function updateStatistics() {
        const params = getFilterParams();
        const queryString = $.param(params);

        $.ajax({
            url: `${API.getStatistics}?${queryString}`,
            method: 'GET',
            success: function (data) {
                $('#statTotal').text(data.totalVehicles);
                $('#statInProgress').text(data.inProgressCount);
                $('#statCompleted').text(data.completedCount);
                $('#statRevenue').text(formatCurrency(data.totalRevenue));
            },
            error: function () {
                console.error('Failed to load statistics');
            }
        });
    }

    /**
     * Show vehicle detail modal
     */
    function showVehicleDetail(id) {
        showLoading();

        $.ajax({
            url: `${API.getDetail}?id=${id}`,
            method: 'GET',
            success: function (response) {
                hideLoading();

                if (response.success) {
                    populateDetailModal(response.data);
                    openModal('detailModal');
                } else {
                    showNotification(response.message || 'Không thể tải thông tin chi tiết', 'error');
                }
            },
            error: function () {
                hideLoading();
                showNotification('Đã xảy ra lỗi khi tải thông tin chi tiết', 'error');
            }
        });
    }

    /**
     * Populate detail modal with vehicle data
     */
    function populateDetailModal(data) {
        // Entry info
        $('#detailBienSoVao').text(data.bienSoVao || '--');
        $('#detailThoiGianVao').text(data.thoiGianVao ? formatDateTime(data.thoiGianVao) : '--');
        $('#detailMaThe').text(data.maThe || '--');

        // Entry image
        if (data.hinhAnhVao) {
            $('#imgVao').attr('src', data.hinhAnhVao).addClass('show');
            $('#imgVaoPlaceholder').addClass('hide');
        } else {
            $('#imgVao').removeClass('show').attr('src', '');
            $('#imgVaoPlaceholder').removeClass('hide');
        }

        // Exit info
        $('#detailBienSoRa').text(data.bienSoRa || '--');
        $('#detailThoiGianRa').text(data.thoiGianRa ? formatDateTime(data.thoiGianRa) : '--');

        // Exit image
        if (data.hinhAnhRa) {
            $('#imgRa').attr('src', data.hinhAnhRa).addClass('show');
            $('#imgRaPlaceholder').addClass('hide');
        } else {
            $('#imgRa').removeClass('show').attr('src', '');
            $('#imgRaPlaceholder').removeClass('hide');
        }

        // Summary
        $('#detailLoaiXe').text(data.tenLoaiXe || '--');
        $('#detailViTri').text(data.tenViTri || '--');
        $('#detailKhuVuc').text(data.tenKhuVuc || '--');
        $('#detailThoiGianGui').text(data.thoiGianGuiFormatted || '--');
        $('#detailTongTien').text(data.tongTien !== null ? formatCurrency(data.tongTien) : '--');

        // Status
        const statusHtml = data.trangThai === 0
            ? '<span class="status-badge in-progress">Đang gửi</span>'
            : '<span class="status-badge completed">Đã lấy xe</span>';
        $('#detailTrangThai').html(statusHtml);
    }

    /**
     * Format datetime string
     */
    function formatDateTime(dateString) {
        if (!dateString) return '--';
        const date = new Date(dateString);
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        return `${day}/${month}/${year} ${hours}:${minutes}`;
    }

    /**
     * Format currency
     */
    function formatCurrency(amount) {
        if (amount === null || amount === undefined) return '--';
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND',
            minimumFractionDigits: 0
        }).format(amount);
    }

    /**
     * Export to Excel (placeholder)
     */
    function exportExcel() {
        showNotification('Tính năng xuất Excel đang được phát triển', 'info');
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
        openModal: openModal,
        closeModal: closeModal,
        exportExcel: exportExcel,
        reloadData: reloadData
    };
})();
