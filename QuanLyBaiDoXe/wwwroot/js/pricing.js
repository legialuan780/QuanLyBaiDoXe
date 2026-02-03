// Pricing Management JavaScript
const Pricing = {
    table: null,
    blockCounter: { add: 0, edit: 0 },
    deleteId: null,

    init: function () {
        this.initDataTable();
        this.bindEvents();
    },

    initDataTable: function () {
        if ($.fn.DataTable.isDataTable('#pricingTable')) {
            $('#pricingTable').DataTable().destroy();
        }

        this.table = $('#pricingTable').DataTable({
            ajax: {
                url: '/Admin/Pricing/GetPricingConfigs',
                dataSrc: 'data'
            },
            columns: [
                {
                    data: null,
                    render: function (data, type, row, meta) {
                        return meta.row + 1;
                    }
                },
                { data: 'tenCauHinh' },
                {
                    data: 'tenLoaiXe',
                    render: function (data) {
                        return data || '<span class="text-muted">Chưa gán</span>';
                    }
                },
                {
                    data: null,
                    render: function (data) {
                        if (data.gioBatDau && data.gioKetThuc) {
                            return `<i class="fas fa-clock text-primary"></i> ${data.gioBatDau} - ${data.gioKetThuc}`;
                        }
                        return '<span class="text-muted">Cả ngày</span>';
                    }
                },
                {
                    data: 'soBlock',
                    className: 'text-center'
                },
                {
                    data: 'isUuTien',
                    render: function (data, type, row) {
                        if (data) {
                            return '<span class="priority-badge-table active"><i class="fas fa-star"></i> Ưu tiên</span>';
                        }
                        return '<span class="priority-badge-table inactive">Không</span>';
                    }
                },
                {
                    data: null,
                    render: function (data) {
                        return `
                            <div class="action-buttons">
                                <button class="btn-icon view" onclick="Pricing.openDetailModal(${data.maCauHinh})" title="Xem chi tiết">
                                    <i class="fas fa-eye"></i>
                                </button>
                                <button class="btn-icon edit" onclick="Pricing.openEditModal(${data.maCauHinh})" title="Sửa">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn-icon priority" onclick="Pricing.togglePriority(${data.maCauHinh})" title="Bật/tắt ưu tiên">
                                    <i class="fas fa-star"></i>
                                </button>
                                <button class="btn-icon delete" onclick="Pricing.confirmDelete(${data.maCauHinh}, '${data.tenCauHinh}')" title="Xóa">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>
                        `;
                    }
                }
            ],
            language: {
                lengthMenu: "Hiển thị _MENU_ dòng",
                zeroRecords: "Không có dữ liệu",
                info: "Hiển thị _START_ - _END_ / _TOTAL_ cấu hình",
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
            order: [[1, 'asc']]
        });
    },

    bindEvents: function () {
        $('#addForm').on('submit', (e) => {
            e.preventDefault();
            this.create();
        });

        $('#editForm').on('submit', (e) => {
            e.preventDefault();
            this.update();
        });

        $('#confirmDeleteBtn').on('click', () => {
            this.delete();
        });

        // Close modal on overlay click
        $('.modal-overlay').on('click', function (e) {
            if (e.target === this) {
                Pricing.closeModal(this.id);
            }
        });
    },

    openModal: function (modalId) {
        $(`#${modalId}`).addClass('active');
    },

    closeModal: function (modalId) {
        $(`#${modalId}`).removeClass('active');
    },

    openAddModal: function () {
        $('#addForm')[0].reset();
        $('#addBlocksContainer').empty();
        this.blockCounter.add = 0;
        this.addBlock('add'); // Thêm 1 block mặc định
        this.openModal('addModal');
    },

    openEditModal: async function (id) {
        try {
            const response = await fetch(`/Admin/Pricing/GetPricingConfig?id=${id}`);
            const result = await response.json();

            if (result.success) {
                const data = result.data;
                $('#editMaCauHinh').val(data.maCauHinh);
                $('#editTenCauHinh').val(data.tenCauHinh);
                $('#editMaLoaiXe').val(data.maLoaiXe);
                $('#editGioBatDau').val(data.gioBatDau);
                $('#editGioKetThuc').val(data.gioKetThuc);
                $('#editIsUuTien').prop('checked', data.isUuTien);

                // Load blocks
                $('#editBlocksContainer').empty();
                this.blockCounter.edit = 0;

                if (data.chiTietGia && data.chiTietGia.length > 0) {
                    data.chiTietGia.forEach(block => {
                        this.addBlock('edit', block);
                    });
                } else {
                    this.addBlock('edit');
                }

                this.openModal('editModal');
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi tải dữ liệu', 'error');
        }
    },

    openDetailModal: async function (id) {
        try {
            const response = await fetch(`/Admin/Pricing/GetPricingConfig?id=${id}`);
            const result = await response.json();

            if (result.success) {
                const data = result.data;
                $('#detailTenCauHinh').text(data.tenCauHinh);
                $('#detailLoaiXe').text(data.tenLoaiXe || 'Chưa gán');

                if (data.gioBatDau && data.gioKetThuc) {
                    $('#detailThoiGian').text(`${data.gioBatDau} - ${data.gioKetThuc}`);
                } else {
                    $('#detailThoiGian').text('Cả ngày');
                }

                $('#detailUuTien').html(data.isUuTien
                    ? '<span class="priority-badge-table active"><i class="fas fa-star"></i> Ưu tiên</span>'
                    : '<span class="priority-badge-table inactive">Không</span>');

                // Load blocks table
                let blocksHtml = '';
                if (data.chiTietGia && data.chiTietGia.length > 0) {
                    data.chiTietGia.forEach(block => {
                        blocksHtml += `
                            <tr>
                                <td>Block ${block.thuTuBlock}</td>
                                <td>${block.soPhutCuaBlock} phút</td>
                                <td>${this.formatCurrency(block.giaTien)}đ</td>
                                <td>${block.isLuyTien ? '<i class="fas fa-check text-success"></i> Có' : '<i class="fas fa-times text-danger"></i> Không'}</td>
                            </tr>
                        `;
                    });
                } else {
                    blocksHtml = '<tr><td colspan="4" class="text-center text-muted">Chưa có block nào</td></tr>';
                }
                $('#detailBlocksTable').html(blocksHtml);

                this.openModal('detailModal');
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi tải dữ liệu', 'error');
        }
    },

    addBlock: function (formType, data = null) {
        this.blockCounter[formType]++;
        const blockNum = this.blockCounter[formType];
        const containerId = `${formType}BlocksContainer`;

        const blockHtml = `
            <div class="block-row" data-block="${blockNum}">
                <div class="block-number">Block ${blockNum}</div>
                <input type="number" class="block-minutes" placeholder="Số phút" min="1" 
                       value="${data ? data.soPhutCuaBlock : ''}" required />
                <input type="number" class="block-price" placeholder="Giá tiền (VNĐ)" min="0" 
                       value="${data ? data.giaTien : ''}" required />
                <label class="block-checkbox">
                    <input type="checkbox" class="block-progressive" ${data && data.isLuyTien ? 'checked' : ''} />
                    <span>Lũy tiến</span>
                </label>
                <button type="button" class="btn-remove-block" onclick="Pricing.removeBlock(this)">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        `;

        $(`#${containerId}`).append(blockHtml);
    },

    removeBlock: function (btn) {
        const blockRow = $(btn).closest('.block-row');
        const container = blockRow.parent();

        if (container.children().length > 1) {
            blockRow.remove();
            this.reorderBlocks(container);
        } else {
            this.showNotification('Phải có ít nhất 1 block!', 'warning');
        }
    },

    reorderBlocks: function (container) {
        container.find('.block-row').each((index, row) => {
            $(row).attr('data-block', index + 1);
            $(row).find('.block-number').text(`Block ${index + 1}`);
        });
    },

    getBlocksData: function (formType) {
        const blocks = [];
        const containerId = `${formType}BlocksContainer`;

        $(`#${containerId} .block-row`).each((index, row) => {
            const $row = $(row);
            blocks.push({
                thuTuBlock: index + 1,
                soPhutCuaBlock: parseInt($row.find('.block-minutes').val()) || 0,
                giaTien: parseFloat($row.find('.block-price').val()) || 0,
                isLuyTien: $row.find('.block-progressive').is(':checked')
            });
        });

        return blocks;
    },

    create: async function () {
        const blocks = this.getBlocksData('add');

        if (blocks.length === 0) {
            this.showNotification('Vui lòng thêm ít nhất 1 block!', 'warning');
            return;
        }

        const data = {
            tenCauHinh: $('#addTenCauHinh').val(),
            maLoaiXe: parseInt($('#addMaLoaiXe').val()),
            gioBatDau: $('#addGioBatDau').val(),
            gioKetThuc: $('#addGioKetThuc').val(),
            isUuTien: $('#addIsUuTien').is(':checked'),
            chiTietGia: blocks
        };

        try {
            const response = await fetch('/Admin/Pricing/Create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification(result.message, 'success');
                this.closeModal('addModal');
                this.table.ajax.reload();
                this.updateStats();
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi thêm cấu hình', 'error');
        }
    },

    update: async function () {
        const blocks = this.getBlocksData('edit');

        if (blocks.length === 0) {
            this.showNotification('Vui lòng thêm ít nhất 1 block!', 'warning');
            return;
        }

        const data = {
            maCauHinh: parseInt($('#editMaCauHinh').val()),
            tenCauHinh: $('#editTenCauHinh').val(),
            maLoaiXe: parseInt($('#editMaLoaiXe').val()),
            gioBatDau: $('#editGioBatDau').val(),
            gioKetThuc: $('#editGioKetThuc').val(),
            isUuTien: $('#editIsUuTien').is(':checked'),
            chiTietGia: blocks
        };

        try {
            const response = await fetch('/Admin/Pricing/Update', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification(result.message, 'success');
                this.closeModal('editModal');
                this.table.ajax.reload();
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi cập nhật cấu hình', 'error');
        }
    },

    confirmDelete: function (id, name) {
        this.deleteId = id;
        $('#deleteConfigName').text(name);
        this.openModal('deleteModal');
    },

    delete: async function () {
        if (!this.deleteId) return;

        try {
            const response = await fetch(`/Admin/Pricing/Delete?id=${this.deleteId}`, {
                method: 'DELETE'
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification(result.message, 'success');
                this.closeModal('deleteModal');
                this.table.ajax.reload();
                this.updateStats();
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi xóa cấu hình', 'error');
        }

        this.deleteId = null;
    },

    togglePriority: async function (id) {
        try {
            const response = await fetch(`/Admin/Pricing/TogglePriority?id=${id}`, {
                method: 'POST'
            });

            const result = await response.json();

            if (result.success) {
                this.showNotification(result.message, 'success');
                this.table.ajax.reload();
                this.updateStats();
            } else {
                this.showNotification(result.message, 'error');
            }
        } catch (error) {
            this.showNotification('Lỗi khi cập nhật', 'error');
        }
    },

    updateStats: async function () {
        try {
            const response = await fetch('/Admin/Pricing/GetPricingConfigs');
            const result = await response.json();

            if (result.data) {
                $('#statTotalConfigs').text(result.data.length);
                $('#statActiveConfigs').text(result.data.filter(c => c.isUuTien).length);
            }
        } catch (error) {
            console.error('Error updating stats:', error);
        }
    },

    formatCurrency: function (value) {
        if (!value) return '0';
        return new Intl.NumberFormat('vi-VN').format(value);
    },

    showNotification: function (message, type) {
        // Simple notification - you can replace with your preferred notification library
        const bgColor = type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : '#ffc107';
        const textColor = type === 'warning' ? '#212529' : '#fff';

        const notification = $(`
            <div style="position: fixed; top: 20px; right: 20px; z-index: 9999; padding: 15px 25px; 
                        background: ${bgColor}; color: ${textColor}; border-radius: 8px; 
                        box-shadow: 0 4px 12px rgba(0,0,0,0.15); animation: slideIn 0.3s ease;">
                ${message}
            </div>
        `);

        $('body').append(notification);
        setTimeout(() => notification.fadeOut(() => notification.remove()), 3000);
    }
};

// Initialize when document is ready
$(document).ready(function () {
    if ($('#pricingTable').length) {
        Pricing.init();
    }
});
