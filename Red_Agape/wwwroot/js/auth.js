document.addEventListener('DOMContentLoaded', function () {

    /* ==========================================
       ICONOS LUCIDE
    ========================================== */

    // Lee la librería que cargó unpkg en window. El comentario le dice al editor que window puede tener cualquier propiedad.
    const lucideLib = /** @type {any} */ (window).lucide;

    // Si la librería cargó, convierte cada <i data-lucide="..."> en un icono SVG.
    if (lucideLib) {
        lucideLib.createIcons();
    }

    /* ==========================================
        MENÚ DE USUARIO
    ========================================== */
    const btnNav = document.getElementById('btnNavToggle');
    const navMenu = document.getElementById('navMenu');
    const btnToggle = document.getElementById('btnUserToggle');
    const dropdownBox = document.getElementById('authDropdownBox');
    const wrapper = document.getElementById('userMenuWrapper');

    function cerrarNav() {
        navMenu?.classList.remove('open');
        btnNav?.setAttribute('aria-expanded', 'false');
    }

    if (btnNav && navMenu) {
        btnNav.addEventListener('click', function (e) {
            e.stopPropagation();
            const abierto = navMenu.classList.toggle('open');
            btnNav.setAttribute('aria-expanded', abierto);
            dropdownBox?.classList.remove('show');   // cierra el de usuario
        });

        // Al tocar un enlace, el menú se cierra
        navMenu.addEventListener('click', function (e) {
            if (e.target.closest('a')) cerrarNav();
        });
    }

    if (btnToggle && dropdownBox) {

        btnToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            cerrarNav();  // cierra el menú de navegación si estaba abierto
            dropdownBox.classList.toggle('show');
        });

        dropdownBox.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    }

    // Clic fuera: cierra ambos
    document.addEventListener('click', function (e) {
        if (wrapper && !wrapper.contains(e.target)) {
            dropdownBox?.classList.remove('show');
        }
        if (navMenu && !navMenu.contains(e.target) && !btnNav?.contains(e.target)) {
            cerrarNav();
        }
    });

    /* ==========================================
       CAMBIAR PESTAÑAS LOGIN / REGISTRO
    ========================================== */

    document.querySelectorAll('[data-auth-tab]').forEach(button => {

        button.addEventListener('click', function () {

            const tabName = button.dataset.authTab;

            document.querySelectorAll('.auth-tab-pane')
                .forEach(p => p.classList.remove('active'));

            document.querySelectorAll('.auth-tab-btn')
                .forEach(b => b.classList.remove('active'));

            document.getElementById('tab-' + tabName)
                ?.classList.add('active');

            button.classList.add('active');
        });
    });

    /* ==========================================
       MOSTRAR / OCULTAR CONTRASEÑA
    ========================================== */

    document.querySelectorAll('[data-toggle-password]').forEach(button => {

        button.addEventListener('click', function () {

            const inputId = button.dataset.togglePassword;
            const input = document.getElementById(inputId);

            if (!input) return;

            if (input.type === 'password') {
                input.type = 'text';
                button.innerHTML = svgOjoTachado;
                button.title = 'Ocultar contraseña';
            } else {
                input.type = 'password';
                button.innerHTML = svgOjoAbierto;
                button.title = 'Mostrar contraseña';
            }
        });
    });

    /* ==========================================
       SEGURIDAD DE CONTRASEÑA
    ========================================== */

    document.querySelectorAll('[data-password-check]').forEach(input => {

        input.addEventListener('input', verificarSeguridadPassword);
    });

    /* ==========================================
       VALIDAR FORMULARIO DE REGISTRO
    ========================================== */

    const registerForm = document.getElementById('registerForm');

    if (registerForm) {

        registerForm.addEventListener('submit', function (e) {

            if (!verificarSeguridadPassword()) {

                e.preventDefault();

                alert('Por favor asegúrate de que la contraseña cumpla con todos los requisitos de seguridad y que ambas coincidan.');
            }
        });
    }
});


/* ==========================================
   ICONOS PARA MOSTRAR CONTRASEÑA
========================================== */

const svgOjoAbierto = '<svg viewBox="0 0 24 24"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path><circle cx="12" cy="12" r="3"></circle></svg>';

const svgOjoTachado = '<svg viewBox="0 0 24 24"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"></path><line x1="1" y1="1" x2="23" y2="23"></line></svg>';


/* ==========================================
   SEGURIDAD DE CONTRASEÑA
========================================== */

function verificarSeguridadPassword() {

    const pwd = document.getElementById('regPassword')?.value || '';
    const confirmPwd = document.getElementById('regConfirmPassword')?.value || '';

    const tieneLongitud = pwd.length >= 8;
    const tieneMayuscula = /[A-ZÁÉÍÓÚÑ]/.test(pwd);
    const tieneEspecial = /[@#$%!^&*()_+\-=[\]{};':"\\|,.<>/?~`]/.test(pwd);
    const coinciden = pwd.length > 0 && pwd === confirmPwd;

    actualizarItemRegla('rule-length', tieneLongitud, 'Mínimo 8 caracteres');
    actualizarItemRegla('rule-upper', tieneMayuscula, 'Al menos una letra mayúscula (A-Z)');
    actualizarItemRegla('rule-special', tieneEspecial, 'Un carácter especial (@, #, $, %, !, etc.)');
    actualizarItemRegla('rule-match', coinciden, 'Las contraseñas coinciden');

    return tieneLongitud && tieneMayuscula && tieneEspecial && coinciden;
}


/* ==========================================
   ACTUALIZAR REQUISITOS VISUALES
========================================== */

function actualizarItemRegla(idElemento, cumple, texto) {

    const li = document.getElementById(idElemento);

    if (!li) return;

    if (cumple) {
        li.classList.add('valid');
        li.textContent = '✔ ' + texto;
    } else {
        li.classList.remove('valid');
        li.textContent = '○ ' + texto;
    }
}