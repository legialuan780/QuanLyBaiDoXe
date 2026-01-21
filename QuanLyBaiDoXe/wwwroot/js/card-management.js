// Card Management JavaScript with AJAX
(function ($) {
    'use strict';

    // Global variables
    let cardsTable = null;
    let vehicleTypes = [];

    // Initialize
    $(document).ready(function () {
        initializeDataTable();
        loadVehicleTypes();
        bindEvents();
    });

    // Initialize DataTable
    function initializeDataTable() {
        cardsTable = $('#cardsTable').DataTable({
            processing: true,
            serverSide: false,
            ajax: {
                url: '/Admin/Card/Search',
                type: 'GET',
                data: function (d) {
                    return {
                        keyword: $('#searchKeyword').val(),
                        loaiThe: $('#filterLoaiThe').val() || null,
                        loaiXe: $('#filterLoaiXe').val() || null,
                        trangThai: $('#filterTrangThai').val() || null
                    };
                },
                dataSrc: function (json) {
                    updateStatistics();
                    return json;
                }
            },
            columns: [
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row, meta) {
                        return meta.row + 1;
                    }
                },
                {
                    data: 'maThe',
                    render: function (data) {
                        return `<span class="card-code">${data}</span>`;
                    }
                },
                {
                    data: 'tenLoaiXe',
                    render: function (data) {
                        return data || 'Chưa xác định';
                    }
                },
                {
                    data: 'loaiThe',
                    render: function (data) {
                        const label = data === 1 ? 'Vé tháng' : 'Vé lượt';
                        const cssClass = data === 1 ? 'monthly' : 'single';
                        return `<span class="card-type-badge ${cssClass}">${label}</span>`;
                    }
                },
                {
                    data: null,
                    render: function (data, type, row) {
                        if (row.dangSuDung) {
                            return '<span class="status-badge in-use">Đang sử dụng</span>';
                        } else if (row.trangThai === 1) {
                            return '<span class="status-badge active">Hoạt động</span>';
                        } else {
                            return '<span class="status-badge locked">Đã khóa</span>';
                        }
                    }
                },
                {
                    data: null,
                    orderable: false,
                    render: function (data, type, row) {
                        let actions = '<div class="action-buttons">';
                        
                        // Edit button
                        actions += `<button class="btn-action btn-action-edit" onclick="CardManager.editCard('${row.maThe}')" title="Sửa">
                                        <i class="fas fa-edit"></i>
                                    </button>`;
                        
                        // Lock/Unlock button
                        if (row.trangThai === 1) {
                            const disabled = row.dangSuDung ? 'disabled' : '';
                            actions += `<button class="btn-action btn-action-lock" onclick="CardManager.lockCard('${row.maThe}')" title="Khóa" ${disabled}>
                                            <i class="fas fa-lock"></i>
                                        </button>`;
                        } else {
                            actions += `<button class="btn-action btn-action-unlock" onclick="CardManager.unlockCard('${row.maThe}')" title="Mở khóa">
                                            <i class="fas fa-unlock"></i>
                                        </button>`;
                        }
                        
                        // Delete button
                        const disabled = row.dangSuDung ? 'disabled' : '';
                        actions += `<button class="btn-action btn-action-delete" onclick="CardManager.deleteCard('${row.maThe}')" title="Xóa" ${disabled}>
                                        <i class="fas fa-trash"></i>
                                    </button>`;
                        
                        actions += '</div>';
                        return actions;
                    }
                }
            ],
            language: {
                processing: 'Đang tải...',
                search: 'Tìm kiếm:',
                lengthMenu: 'Hiển thị _MENU_ thẻ',
                info: 'Hiển thị _START_ đến _END_ trong tổng số _TOTAL_ thẻ',
                infoEmpty: 'Không có dữ liệu',
                infoFiltered: '(lọc từ _MAX_ thẻ)',
                zeroRecords: 'Không tìm thấy thẻ nào',
                emptyTable: 'Không có dữ liệu',
                paginate: {
                    first: 'Đầu',
                    previous: 'Trước',
                    next: 'Sau',
                    last: 'Cuối'
                }
            },
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]],
            order: [[1, 'asc']],
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                 '<"row"<"col-sm-12"tr>>' +
                 '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>'
        });
    }

    // Load vehicle types
    function loadVehicleTypes() {
        // Get vehicle types from existing select options
        vehicleTypes = [];
        $('#filterLoaiXe option').each(function() {
            const value = $(this).val();
            const text = $(this).text();
            if (value) {
                vehicleTypes.push({ value: value, text: text });
            }
        });
    }

    // Bind events
    function bindEvents() {
        // Filter form submit
        $('#filterForm').on('submit', function (e) {
            e.preventDefault();
            cardsTable.ajax.reload();
        });

        // Reset filter
        $('#btnResetFilter').on('click', function () {
            $('#searchKeyword').val('');
            $('#filterLoaiThe').val('');
            $('#filterLoaiXe').val('');
            $('#filterTrangThai').val('');
            cardsTable.ajax.reload();
        });

        // Modal close on overlay click
        $('.modal-overlay').on('click', function (e) {
            if (e.target === this) {
                $(this).removeClass('show');
            }
        });

        // Form submissions
        $('#addCardForm').on('submit', handleAddCard);
        $('#importCardForm').on('submit', handleImportCards);
        $('#editCardForm').on('submit', handleEditCard);

        // Import preview update
        $('#importPrefix, #importStartNumber, #importQuantity').on('input', updateImportPreview);
        updateImportPreview();
    }

    // Update statistics
    function updateStatistics() {
        $.get('/Admin/Card/GetStatistics', function (data) {
            $('#statTotal').text(data.totalCards);
            $('#statActive').text(data.activeCards);
            $('#statInUse').text(data.cardsInUse);
            $('#statLocked').text(data.lockedCards);
        });
    }

    // Modal functions
    window.CardManager = {
        openModal: function (modalId) {
            $('#' + modalId).addClass('show');
        },

        closeModal: function (modalId) {
            $('#' + modalId).removeClass('show');
        },

        showNotification: function (message, type) {
            const $notification = $('#notification');
            $notification.text(message).removeClass('success error').addClass(type + ' show');
            setTimeout(function () {
                $notification.removeClass('show');
            }, 3000);
        },

        showLoading: function () {
            $('#loadingOverlay').addClass('show');
        },

        hideLoading: function () {
            $('#loadingOverlay').removeClass('show');
        },

        editCard: async function (maThe) {
            try {
                this.showLoading();
                const response = await fetch(`/Admin/Card/GetCard?maThe=${encodeURIComponent(maThe)}`);
                const result = await response.json();
                this.hideLoading();

                if (result.success) {
                    $('#editMaThe').val(result.card.maThe);
                    $('#editMaTheDisplay').val(result.card.maThe);
                    $('#editMaLoaiXe').val(result.card.maLoaiXe || '');
                    $('#editLoaiThe').val(result.card.loaiThe || 0);
                    $('#editTrangThai').val(result.card.trangThai || 1);
                    this.openModal('editModal');
                } else {
                    this.showNotification(result.message, 'error');
                }
            } catch (err) {
                this.hideLoading();
                this.showNotification('Lỗi kết nối server!', 'error');
            }
        },

        lockCard: async function (maThe) {
            if (!confirm(`Bạn có chắc muốn khóa thẻ ${maThe}?`)) return;

            try {
                this.showLoading();
                const response = await fetch(`/Admin/Card/Lock?maThe=${encodeURIComponent(maThe)}`, {
                    method: 'POST'
                });
                const result = await response.json();
                this.hideLoading();

                if (result.success) {
                    this.showNotification(result.message, 'success');
                    cardsTable.ajax.reload(null, false);
                } else {
                    this.showNotification(result.message, 'error');
                }
            } catch (err) {
                this.hideLoading();
                this.showNotification('Lỗi kết nối server!', 'error');
            }
        },

        unlockCard: async function (maThe) {
            if (!confirm(`Bạn có chắc muốn mở khóa thẻ ${maThe}?`)) return;

            try {
                this.showLoading();
                const response = await fetch(`/Admin/Card/Unlock?maThe=${encodeURIComponent(maThe)}`, {
                    method: 'POST'
                });
                const result = await response.json();
                this.hideLoading();

                if (result.success) {
                    this.showNotification(result.message, 'success');
                    cardsTable.ajax.reload(null, false);
                } else {
                    this.showNotification(result.message, 'error');
                }
            } catch (err) {
                this.hideLoading();
                this.showNotification('Lỗi kết nối server!', 'error');
            }
        },

        deleteCard: async function (maThe) {
            if (!confirm(`Bạn có chắc muốn xóa thẻ ${maThe}? Hành động này không thể hoàn tác!`)) return;

            try {
                this.showLoading();
                const response = await fetch(`/Admin/Card/Delete?maThe=${encodeURIComponent(maThe)}`, {
                    method: 'POST'
                });
                const result = await response.json();
                this.hideLoading();

                if (result.success) {
                    this.showNotification(result.message, 'success');
                    cardsTable.ajax.reload();
                } else {
                    this.showNotification(result.message, 'error');
                }
            } catch (err) {
                this.hideLoading();
                this.showNotification('Lỗi kết nối server!', 'error');
            }
        }
    };

    // Handle add card
    async function handleAddCard(e) {
        e.preventDefault();

        const data = {
            maThe: $('#addMaThe').val().trim().toUpperCase(),
            maLoaiXe: parseInt($('#addMaLoaiXe').val()),
            loaiThe: parseInt($('#addLoaiThe').val()),
            trangThai: 1
        };

        try {
            CardManager.showLoading();
            const response = await fetch('/Admin/Card/Create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            const result = await response.json();
            CardManager.hideLoading();

            if (result.success) {
                CardManager.showNotification(result.message, 'success');
                CardManager.closeModal('addModal');
                $('#addCardForm')[0].reset();
                cardsTable.ajax.reload();
            } else {
                CardManager.showNotification(result.message, 'error');
            }
        } catch (err) {
            CardManager.hideLoading();
            CardManager.showNotification('Lỗi kết nối server!', 'error');
        }
    }

    // Handle import cards
    async function handleImportCards(e) {
        e.preventDefault();

        const data = {
            prefix: $('#importPrefix').val().trim().toUpperCase(),
            startNumber: parseInt($('#importStartNumber').val()),
            quantity: parseInt($('#importQuantity').val()),
            maLoaiXe: parseInt($('#importMaLoaiXe').val()),
            loaiThe: parseInt($('#importLoaiThe').val())
        };

        try {
            CardManager.showLoading();
            const response = await fetch('/Admin/Card/CreateMultiple', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            const result = await response.json();
            CardManager.hideLoading();

            if (result.success) {
                CardManager.showNotification(result.message, 'success');
                CardManager.closeModal('importModal');
                $('#importCardForm')[0].reset();
                cardsTable.ajax.reload();
            } else {
                CardManager.showNotification(result.message, 'error');
            }
        } catch (err) {
            CardManager.hideLoading();
            CardManager.showNotification('Lỗi kết nối server!', 'error');
        }
    }

    // Handle edit card
    async function handleEditCard(e) {
        e.preventDefault();

        const data = {
            maThe: $('#editMaThe').val(),
            maLoaiXe: parseInt($('#editMaLoaiXe').val()),
            loaiThe: parseInt($('#editLoaiThe').val()),
            trangThai: parseInt($('#editTrangThai').val())
        };

        try {
            CardManager.showLoading();
            const response = await fetch('/Admin/Card/Update', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });
            const result = await response.json();
            CardManager.hideLoading();

            if (result.success) {
                CardManager.showNotification(result.message, 'success');
                CardManager.closeModal('editModal');
                cardsTable.ajax.reload(null, false);
            } else {
                CardManager.showNotification(result.message, 'error');
            }
        } catch (err) {
            CardManager.hideLoading();
            CardManager.showNotification('Lỗi kết nối server!', 'error');
        }
    }

    // Update import preview
    function updateImportPreview() {
        const prefix = $('#importPrefix').val().toUpperCase() || 'THE';
        const start = parseInt($('#importStartNumber').val()) || 1;
        const quantity = parseInt($('#importQuantity').val()) || 1;
        const end = start + quantity - 1;

        $('#previewCards').text(`${prefix}${start.toString().padStart(4, '0')} → ${prefix}${end.toString().padStart(4, '0')}`);
    }

})(jQuery);
