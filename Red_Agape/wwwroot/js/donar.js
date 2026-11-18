document.addEventListener('DOMContentLoaded', () => { //cómo WinForms: button.Click += ...
    const contenedor = document.getElementById('contenedor-articulos');
    const plantilla = document.getElementById('plantilla-articulo');
    const btnAgregar = document.getElementById('btn-agregar');

    const form = contenedor.closest('form');

    const articulos = () => contenedor.querySelectorAll('[data-articulo]');


    // Mantiene índices contiguos (0,1,2...) en name, id, for y data-valmsg-for, para que si se elimina uno intermedio, los demás se reindexen correctamente
    function reindexar() {
        articulos().forEach((art, i) => {
            art.querySelector('.num').textContent = i + 1;

            art.querySelectorAll('[name]').forEach(el => {
                el.name = el.name.replace(/Articulos\[\d+\]/, `Articulos[${i}]`);
                if (el.id) el.id = el.id.replace(/Articulos_\d+__/, `Articulos_${i}__`);
            });
            art.querySelectorAll('label[for]').forEach(el => {
                el.htmlFor = el.htmlFor.replace(/Articulos_\d+__/, `Articulos_${i}__`);
            });
            art.querySelectorAll('[data-valmsg-for]').forEach(el => {
                el.setAttribute('data-valmsg-for',
                    el.getAttribute('data-valmsg-for').replace(/Articulos\[\d+\]/, `Articulos[${i}]`));
            });
        });

        // No permitir quitar el último artículo
        const unico = articulos().length === 1;
        contenedor.querySelectorAll('.btn-quitar').forEach(b => b.disabled = unico);
    }

    // Vuelve a registrar la validación de cliente para los inputs nuevos
    function revalidar() {
        if (window.jQuery && jQuery.validator && jQuery.validator.unobtrusive) {
            jQuery(form).removeData('validator').removeData('unobtrusiveValidation');
            jQuery.validator.unobtrusive.parse(form);
        }
    }

    btnAgregar.addEventListener('click', () => {
        const html = plantilla.innerHTML.replaceAll('__INDEX__', articulos().length);
        contenedor.insertAdjacentHTML('beforeend', html);
        reindexar();
        revalidar();
        articulos()[articulos().length - 1].querySelector('input')?.focus();
    });

    // Delegación de eventos: funciona con artículos existentes y nuevos
    contenedor.addEventListener('click', (e) => {
        const btn = e.target.closest('.btn-quitar');
        if (!btn || articulos().length === 1) return;
        btn.closest('[data-articulo]').remove();
        reindexar();
        revalidar();
    });

// Muestra la imagen elegida y el botón "Quitar foto"
function mostrarFoto(campo) {
    const input = campo.querySelector('input[type="file"]');
    const img = campo.querySelector('.foto-preview');
    const btn = campo.querySelector('.btn-quitar-foto');

    const archivo = input.files[0];
    if (!archivo) {
        quitarFoto(campo);
        return;
    }

    liberarUrl(img);
    img.src = URL.createObjectURL(archivo);
    img.hidden = false;
    btn.hidden = false;
}

// Limpia el input, oculta la imagen y el botón
function quitarFoto(campo) {
    const input = campo.querySelector('input[type="file"]');
    const img = campo.querySelector('.foto-preview');
    const btn = campo.querySelector('.btn-quitar-foto');

    input.value = '';
    liberarUrl(img);
    img.removeAttribute('src');
    img.hidden = true;
    btn.hidden = true;
}

// Libera la memoria de la URL temporal anterior
function liberarUrl(img) {
    const actual = img.getAttribute('src');
    if (actual && actual.startsWith('blob:')) {
        URL.revokeObjectURL(actual);
    }
}

// Cuando el usuario elige un archivo (artículos existentes y nuevos)
contenedor.addEventListener('change', (e) => {
    const input = e.target.closest('input[type="file"]');
    if (!input) return;
    mostrarFoto(input.closest('.foto-campo'));
});

// Cuando presiona "Quitar foto"
contenedor.addEventListener('click', (e) => {
    const btn = e.target.closest('.btn-quitar-foto');
    if (!btn) return;
    quitarFoto(btn.closest('.foto-campo'));
});

    reindexar();
});