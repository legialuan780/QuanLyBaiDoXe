/**
 * Anomaly Type Helper - Th?ng nh?t icon và màu s?c cho các lo?i s? c?
 * @version 1.0.0
 * @description Helper functions ?? hi?n th? nh?t quán các lo?i s? c? trong toàn b? h? th?ng
 */

const AnomalyTypeHelper = {
    /**
     * C?u hình icon và màu s?c cho t?ng lo?i s? c?
     */
    types: {
        'Xe m?t th?': {
            icon: 'fas fa-id-card-alt',
            color: '#f39c12',
            bgClass: 'bg-warning',
            textClass: 'text-warning',
            label: 'Xe m?t th?'
        },
        'L?i barrier': {
            icon: 'fas fa-road-barrier',
            color: '#e74c3c',
            bgClass: 'bg-danger',
            textClass: 'text-danger',
            label: 'L?i barrier'
        },
        'L?i camera': {
            icon: 'fas fa-video-slash',
            color: '#3498db',
            bgClass: 'bg-info',
            textClass: 'text-info',
            label: 'L?i camera'
        },
        'Tranh ch?p phí': {
            icon: 'fas fa-hand-holding-usd',
            color: '#9b59b6',
            bgClass: 'bg-purple',
            textClass: 'text-purple',
            label: 'Tranh ch?p phí'
        },
        'Kh?n c?p': {
            icon: 'fas fa-exclamation-triangle',
            color: '#e74c3c',
            bgClass: 'bg-danger',
            textClass: 'text-danger',
            label: 'Kh?n c?p',
            pulse: true // Có hi?u ?ng pulse
        },
        'Khác': {
            icon: 'fas fa-flag',
            color: '#95a5a6',
            bgClass: 'bg-secondary',
            textClass: 'text-secondary',
            label: 'Khác'
        }
    },

    /**
     * L?y icon cho lo?i s? c?
     * @param {string} type - Lo?i s? c?
     * @returns {string} Class icon
     */
    getIcon(type) {
        return this.types[type]?.icon || this.types['Khác'].icon;
    },

    /**
     * L?y màu hex cho lo?i s? c?
     * @param {string} type - Lo?i s? c?
     * @returns {string} Mã màu hex
     */
    getColor(type) {
        return this.types[type]?.color || this.types['Khác'].color;
    },

    /**
     * L?y class background cho lo?i s? c?
     * @param {string} type - Lo?i s? c?
     * @returns {string} Bootstrap class
     */
    getBgClass(type) {
        return this.types[type]?.bgClass || this.types['Khác'].bgClass;
    },

    /**
     * L?y class text color cho lo?i s? c?
     * @param {string} type - Lo?i s? c?
     * @returns {string} Bootstrap class
     */
    getTextClass(type) {
        return this.types[type]?.textClass || this.types['Khác'].textClass;
    },

    /**
     * Ki?m tra có c?n hi?u ?ng pulse không
     * @param {string} type - Lo?i s? c?
     * @returns {boolean}
     */
    hasPulse(type) {
        return this.types[type]?.pulse || false;
    },

    /**
     * Render badge HTML cho lo?i s? c?
     * @param {string} type - Lo?i s? c?
     * @returns {string} HTML string
     */
    renderBadge(type) {
        const config = this.types[type] || this.types['Khác'];
        const pulseClass = config.pulse ? ' pulse-animation' : '';
        
        return `
            <span class="badge ${config.bgClass}${pulseClass}">
                <i class="${config.icon} me-1"></i>${config.label}
            </span>
        `;
    },

    /**
     * Render icon v?i tooltip cho lo?i s? c?
     * @param {string} type - Lo?i s? c?
     * @param {string} size - Kích th??c (sm, md, lg)
     * @returns {string} HTML string
     */
    renderIcon(type, size = 'md') {
        const config = this.types[type] || this.types['Khác'];
        const sizeClass = size === 'sm' ? 'fa-sm' : size === 'lg' ? 'fa-lg' : '';
        
        return `
            <i class="${config.icon} ${sizeClass} ${config.textClass}" 
               title="${config.label}"
               data-bs-toggle="tooltip"></i>
        `;
    },

    /**
     * Render option cho select dropdown
     * @returns {string} HTML options
     */
    renderOptions() {
        let html = '<option value="">T?t c? lo?i</option>';
        
        Object.keys(this.types).forEach(type => {
            const config = this.types[type];
            html += `<option value="${type}">${config.label}</option>`;
        });
        
        return html;
    },

    /**
     * L?y danh sách t?t c? các lo?i s? c?
     * @returns {Array} M?ng các lo?i s? c?
     */
    getAllTypes() {
        return Object.keys(this.types);
    },

    /**
     * Format lo?i s? c? cho hi?n th? trong b?ng
     * @param {string} type - Lo?i s? c?
     * @returns {Object} ??i t??ng ch?a thông tin format
     */
    format(type) {
        const config = this.types[type] || this.types['Khác'];
        return {
            icon: config.icon,
            color: config.color,
            bgClass: config.bgClass,
            textClass: config.textClass,
            label: config.label,
            pulse: config.pulse || false,
            badge: this.renderBadge(type),
            iconHtml: this.renderIcon(type)
        };
    }
};

// Export ?? s? d?ng ? các file khác
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AnomalyTypeHelper;
}
