// Monthly Ticket Management JavaScript
const MonthlyTicket = {
    table: null,
    cancelId: null,
    currentTicket: null,

    init: function () {
        this.initDataTable();
        this.initSelect2();
        this.bindEvents();
        this.updateExpiryDate();
    },

    initDataTable: function () {
        if ($.fn.DataTable.isDataTable('#monthlyTicketTable')) {
            $('#monthlyTicketTable').DataTable().destroy();
        }

        this.table = $('#monthlyTicketTable').DataTable({
            ajax: {
                url: '/Admin/MonthlyTicket/GetMonthlyTickets',
                dataSrc: 'data'
            },
            columns: [
                {
                    data: null,
                    render: function (data, type, row, meta) {
                        return meta.row + 1;
                    }
                },
                { data: 'tenKhachHang', defaultContent: '<span class="text-muted">N/A</span>' },
                { data: 'soDienThoai', defaultContent: '<span class="text-muted">N/A</span>' },
                {
                    data: 'bienSoXe',
                    render: function (data) {
                        return data ? `<strong>${data}</strong>` : '<span class="text-muted">Chưa có</span>';
                    }
                },
                {
                    data: 'maThe',
                    render: function (data) {
                        return `<span class="badge bg-primary">${data}</span>`;
                    }
                },
                { data: 'tenLoaiXe', defaultContent: '<span class="text-muted">Chưa phân loại</span>' },
                {
                    data: 'ngayBatDau',
                    render: function (data) {
                        return data ? MonthlyTicket.formatDate(data) : '-';
                    }
                },
                {
                    data: 'ngayHetHan',
                    render: function (data, type, row) {
                        if (!data) return '-';
                        const formatted = MonthlyTicket.formatDate(data);
                        const daysClass = row.soNgayConLai <= 0 ? 'danger' : row.soNgayConLai <= 7 ? 'warning' : 'ok';
                        return `${formatted} <span class="days-badge ${daysClass}">${row.soNgayConLai} ngày</span>`;
                    }
                },
                {
                    data: 'soTienDong',
                    render: function (data) {
                        return data ? MonthlyTicket.formatCurrency(data) + 'đ' : '-';
                    }
                },
                {
                    data: null,
                    render: function (data) {
                        const statusClass = data.trangThai 
                            ? (data.soNgayConLai <= 7 ? 'warning' : 'success') 
                            : 'danger';
                        const statusText = data.trangThai 
                            ? (data.soNgayConLai <= 0 ? 'Hết hạn' : 'Hoạt động') 
                            : 'Đã hủy';
                        const icon = data.trangThai 
                            ? (data.soNgayConLai <= 7 ? 'exclamation-circle' : 'check-circle') 
                            : 'times-circle';
                        return `<span class="status-badge-ticket ${statusClass}"><i class="fas fa-${icon}"></i> ${statusText}</span>`;
                    }
                },
                {
                    data: null,
                    render: function (data) {
                        let buttons = `
                            <div class="action-buttons">
                                <button class="btn-icon view" onclick="MonthlyTicket.viewDetail(${data.maTheThang})" title="Xem chi tiết">
                                    <i class="fas fa-eye"></i>
                                </button>`;
                        
                        if (data.trangThai) {
                            buttons += `
                                <button class="btn-icon renew" onclick="MonthlyTicket.openRenewModal(${data.maTheThang})" title="Gia hạn">
                                    <i class="fas fa-sync-alt"></i>
                                </button>
                                <button class="btn-icon cancel" onclick="MonthlyTicket.confirmCancel(${data.maTheThang}, '${data.tenKhachHang}')" title="Hủy vé">
                                    <i class="fas fa-ban"></i>
                                </button>`;
                        }
                        
                        buttons += '</div>';
                        return buttons;
                    }
                }
            ],
            language: {
                lengthMenu: "Hiển thị _MENU_ dòng",
                zeroRecords: "Không có dữ liệu",
                info: "Hiển thị _START_ - _END_ / _TOTAL_ vé tháng",
                infoEmpty: "Không có dữ liệu",
                infoFiltered: "(lọc từ _MAX_ dòng)",
                search: "Tìm kiếm:",
                paginate: {
                    first: "Đầu",
                    last: "Cuối",
                    next: "Sau",
                    previous: "Trước"
                }
            },
            pageLength: 10,
            responsive: true,
            order: [[6, 'desc']]
        });
    },

    initSelect2: function () {
        $('.select2-customer').select2({
            placeholder: '-- Chọn khách hàng --',
            allowClear: true,
            dropdownParent: $('#registerModal')
        });

        $('.select2-card').select2({
            placeholder: '-- Chọn thẻ --',
            allowClear: true,
            dropdownParent: $('#registerModal')
        });
    },

    bindEvents: function () {
        // Register form
        $('#registerForm').on('submit', (e) => {
            e.preventDefault();
            this.register();
        });

        // Renew form
        $('#renewForm').on('submit', (e) => {
            e.preventDefault();
            this.renew();
        });

        // Cancel confirm button
        $('#confirmCancelBtn').on('click', () => {
            this.cancel();
        });

        // Customer selection change
        $('#regCustomer').on('change', function () {
            const selected = $(this).find(':selected');
            const plate = selected.data('plate');
            $('#regBienSo').val(plate || '');
        });

        // Card selection change
        $('#regCard').on('change', function () {
            const selected = $(this).find(':selected');
            const type = selected.data('type');
            const price = selected.data('price');
            $('#regLoaiXe').val(type || 'Chưa phân loại');
            MonthlyTicket.updatePrice();
        });

        // Months selection change - Register
        $('#regSoThang').on('change', () => {
            this.updateExpiryDate();
            this.updatePrice();
        });

        // Months selection change - Renew
        $('#renewSoThang').on('change', () => {
            this.updateRenewExpiryDate();
            this.updateRenewPrice();
        });

        // Close modal on overlay click
        $('.modal-overlay').on('click', function (e) {
            if (e.target === this) {
                MonthlyTicket.closeModal(this.id);
            }
        });

        // Filter status
        $('#filterStatus').on('change', () => {
            this.filterTable();
        });
    },

    openModal: function (modalId) {
        $(`#${modalId}`).addClass('active');
    },

    closeModal: function (modalId) {
        $(`#${modalId}`).removeClass('active');
    },

    openRegisterModal: function () {
        $('#registerForm')[0].reset();
        $('#regCustomer').val('').trigger('change');
        $('#regCard').val('').trigger('change');
        $('#regLoaiXe').val('');
        $('#regSoTien').val('');
        this.updateExpiryDate();
        this.openModal('registerModal');
    },

    updatePrice: function () {
        const selected = $('#regCard').find(':selected');
        const price = parseFloat(selected.data('price')) || 0;
        const months = parseInt($('#regSoThang').val()) || 1;
        const total = price * months;
        $('#regSoTien').val(total > 0 ? total : '');
    },

    updateExpiryDate: function () {
        const months = parseInt($('#regSoThang').val()) || 1;
        const today = new Date();
        const expiry = new Date(today);
        expiry.setMonth(expiry.getMonth() + months);
        $('#regNgayHetHan').val(this.formatDateVN(expiry));
    },

    openRenewModal: async function (id) {
        try {
            const response = await fetch(`/Admin/MonthlyTicket/GetMonthlyTicket?id=${id}`);
            const result = await response.json();

            if (result.success) {
                const data = result.data;
                this.currentTicket = data;

                $('#renewMaVeThang').val(data.maTheThang);
                $('#renewCustomerName').text(data.tenKhachHang || 'N/A');
                $('#renewBienSo').text(data.bienSoXe || 'Chưa có');
                $('#renewMaThe').text(data.maThe);
                $('#renewCurrentExpiry').text(data.ngayHetHan ? this.formatDate(data.ngayHetHan) : 'N/A');
                $('#renewSoThang').val('1');
                
                this.updateRenewExpiryDate();
                this.updateRenewPrice();
                this.openModal('renewModal');
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi tải dữ liệu', 'error');
        }
    },

    updateRenewExpiryDate: function () {
        if (!this.currentTicket) return;

        const months = parseInt($('#renewSoThang').val()) || 1;
        let startDate;

        if (this.currentTicket.ngayHetHan) {
            const parts = this.currentTicket.ngayHetHan.split('-');
            startDate = new Date(parts[0], parts[1] - 1, parts[2]);
            if (startDate < new Date()) {
                startDate = new Date();
            }
        } else {
            startDate = new Date();
        }

        const expiry = new Date(startDate);
        expiry.setMonth(expiry.getMonth() + months);
        $('#renewNewExpiry').val(this.formatDateVN(expiry));
    },

    updateRenewPrice: function () {
        if (!this.currentTicket) return;
        
        const price = parseFloat(this.currentTicket.giaThang) || 0;
        const months = parseInt($('#renewSoThang').val()) || 1;
        const total = price * months;
        $('#renewSoTien').val(total > 0 ? total : '');
    },

    viewDetail: async function (id) {
        try {
            const response = await fetch(`/Admin/MonthlyTicket/GetMonthlyTicket?id=${id}`);
            const result = await response.json();

            if (result.success) {
                const data = result.data;

                $('#detailCustomerName').text(data.tenKhachHang || 'N/A');
                $('#detailPhone').text(data.soDienThoai || 'N/A');
                $('#detailBienSo').text(data.bienSoXe || 'Chưa có');
                $('#detailMaThe').text(data.maThe);
                $('#detailLoaiXe').text(data.tenLoaiXe || 'Chưa phân loại');
                
                const statusClass = data.trangThai 
                    ? (data.soNgayConLai <= 7 ? 'warning' : 'success') 
                    : 'danger';
                const statusText = data.trangThai 
                    ? (data.soNgayConLai <= 0 ? 'Hết hạn' : 'Hoạt động') 
                    : 'Đã hủy';
                $('#detailStatus').html(`<span class="status-badge-ticket ${statusClass}">${statusText}</span>`);
                
                $('#detailStartDate').text(data.ngayBatDau ? this.formatDate(data.ngayBatDau) : 'N/A');
                $('#detailEndDate').text(data.ngayHetHan ? this.formatDate(data.ngayHetHan) : 'N/A');
                $('#detailRemaining').text(`${data.soNgayConLai} ngày`);
                $('#detailAmount').text(data.soTienDong ? this.formatCurrency(data.soTienDong) + 'đ' : '0đ');

                // Load renewal history
                await this.loadRenewalHistory(id);

                this.openModal('detailModal');
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi tải dữ liệu', 'error');
        }
    },

    loadRenewalHistory: async function (id) {
        try {
            const response = await fetch(`/Admin/MonthlyTicket/GetRenewalHistory?id=${id}`);
            const result = await response.json();

            const tbody = $('#renewalHistoryTable tbody');
            if (result.success && result.data.length > 0) {
                tbody.html(result.data.map(item => `
                    <tr>
                        <td>${item.ngayGiaHan ? new Date(item.ngayGiaHan).toLocaleString('vi-VN') : '-'}</td>
                        <td>${item.thoiHanCu ? this.formatDate(item.thoiHanCu) : '-'}</td>
                        <td>${item.thoiHanMoi ? this.formatDate(item.thoiHanMoi) : '-'}</td>
                        <td>${item.soTien ? this.formatCurrency(item.soTien) + 'đ' : '-'}</td>
                    </tr>
                `).join(''));
            } else {
                tbody.html('<tr><td colspan="4" class="text-center text-muted">Chưa có lịch sử gia hạn</td></tr>');
            }
        } catch (error) {
            console.error('Error loading renewal history:', error);
        }
    },

    register: async function () {
        const data = {
            maKhachHang: parseInt($('#regCustomer').val()),
            maThe: $('#regCard').val(),
            bienSoXe: $('#regBienSo').val(),
            soThang: parseInt($('#regSoThang').val()),
            soTienDong: parseFloat($('#regSoTien').val()) || 0
        };

        try {
            const response = await fetch('/Admin/MonthlyTicket/Create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification(result.message, 'success');
                this.closeModal('registerModal');
                this.table.ajax.reload();
                this.refreshAvailableCards();
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi đăng ký vé tháng', 'error');
        }
    },

    renew: async function () {
        const data = {
            maVeThang: parseInt($('#renewMaVeThang').val()),
            soThang: parseInt($('#renewSoThang').val()),
            soTienDong: parseFloat($('#renewSoTien').val()) || 0
        };

        try {
            const response = await fetch('/Admin/MonthlyTicket/Renew', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification(result.message, 'success');
                this.closeModal('renewModal');
                this.table.ajax.reload();
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi gia hạn vé tháng', 'error');
        }
    },

    confirmCancel: function (id, customerName) {
        this.cancelId = id;
        $('#cancelCustomerName').text(customerName || 'N/A');
        this.openModal('cancelModal');
    },

    cancel: async function () {
        if (!this.cancelId) return;

        try {
            const response = await fetch(`/Admin/MonthlyTicket/Cancel?id=${this.cancelId}`, {
                method: 'POST'
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification(result.message, 'success');
                this.closeModal('cancelModal');
                this.table.ajax.reload();
                this.refreshAvailableCards();
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi hủy vé tháng', 'error');
        }

        this.cancelId = null;
    },

    refreshAvailableCards: async function () {
        try {
            const response = await fetch('/Admin/MonthlyTicket/GetAvailableCards');
            const result = await response.json();

            if (result.success) {
                const select = $('#regCard');
                select.empty();
                select.append('<option value="">-- Chọn thẻ --</option>');
                
                result.data.forEach(card => {
                    select.append(`<option value="${card.maThe}" data-type="${card.tenLoaiXe}">${card.maThe} (${card.tenLoaiXe || 'Chưa phân loại'})</option>`);
                });
            }
        } catch (error) {
            console.error('Error refreshing cards:', error);
        }
    },

    filterTable: function () {
        const status = $('#filterStatus').val();
        
        $.fn.dataTable.ext.search.pop();
        
        if (status) {
            $.fn.dataTable.ext.search.push(function (settings, data, dataIndex) {
                const rowData = MonthlyTicket.table.row(dataIndex).data();
                
                switch (status) {
                    case 'active':
                        return rowData.trangThai && rowData.soNgayConLai > 7;
                    case 'expiring':
                        return rowData.trangThai && rowData.soNgayConLai > 0 && rowData.soNgayConLai <= 7;
                    case 'expired':
                        return !rowData.trangThai || rowData.soNgayConLai <= 0;
                    default:
                        return true;
                }
            });
        }
        
        this.table.draw();
    },

    formatDate: function (dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString('vi-VN');
    },

    formatDateVN: function (date) {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}/${month}/${year}`;
    },

    formatCurrency: function (value) {
        if (!value) return '0';
        return new Intl.NumberFormat('vi-VN').format(value);
    },

    showNotification: function (message, type) {
        const bgColor = type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#ffc107';
        const textColor = type === 'warning' ? '#212529' : '#fff';

        const notification = $(`
            <div style="position: fixed; top: 20px; right: 20px; z-index: 9999; padding: 15px 25px; 
                        background: ${bgColor}; color: ${textColor}; border-radius: 8px; 
                        box-shadow: 0 4px 12px rgba(0,0,0,0.15); animation: slideIn 0.3s ease;">
                <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'exclamation-triangle'}" style="margin-right: 8px;"></i>
                ${message}
            </div>
        `);

        $('body').append(notification);
        setTimeout(() => notification.fadeOut(() => notification.remove()), 3000);
    }
};

// Initialize when document is ready
$(document).ready(function () {
    MonthlyTicket.init();
});
