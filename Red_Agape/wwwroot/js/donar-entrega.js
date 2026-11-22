document.addEventListener('DOMContentLoaded', function () {

    const RADIO_CERCANO_KM = 10;

    const seccionDireccion = document.getElementById('seccion-direccion');
    const selectZona = document.getElementById('Input_ZonaId');
    const selectOrg = document.getElementById('Input_OrgDestinoId');
    const infoOrg = document.getElementById('info-direccion-org');
    const radios = document.querySelectorAll('input[name="Input.TipoEntrega"]');

    const opcionVacia = selectOrg.querySelector('option[value=""]');
    const opcionesOrg = Array.from(selectOrg.options).filter(o => o.value !== '');

    // Guardamos el nombre limpio, porque más adelante le agregamos los km al texto
    opcionesOrg.forEach(o => o.dataset.nombre = o.textContent);

    function distanciaKm(lat1, lng1, lat2, lng2) {
        const R = 6371;
        const aRad = grados => grados * Math.PI / 180;
        const dLat = aRad(lat2 - lat1);
        const dLng = aRad(lng2 - lng1);
        const a = Math.sin(dLat / 2) ** 2 +
            Math.cos(aRad(lat1)) * Math.cos(aRad(lat2)) * Math.sin(dLng / 2) ** 2;
        return 2 * R * Math.asin(Math.sqrt(a));
    }

    function crearGrupo(titulo, opciones) {
        if (opciones.length === 0) return null;

        const grupo = document.createElement('optgroup');
        grupo.label = titulo;
        opciones.forEach(o => grupo.appendChild(o));
        return grupo;
    }

    function ordenarOrganizaciones() {
        const orgActual = selectOrg.value;
        const zona = selectZona.selectedOptions[0];
        const zonaLat = zona ? parseFloat(zona.dataset.lat) : NaN;
        const zonaLng = zona ? parseFloat(zona.dataset.lng) : NaN;
        const hayOrigen = !isNaN(zonaLat) && !isNaN(zonaLng);

        const lista = opcionesOrg.map(o => {
            const lat = parseFloat(o.dataset.lat);
            const lng = parseFloat(o.dataset.lng);
            const tieneUbicacion = hayOrigen && !isNaN(lat) && !isNaN(lng);
            const km = tieneUbicacion ? distanciaKm(zonaLat, zonaLng, lat, lng) : null;

            o.textContent = km === null
                ? o.dataset.nombre
                : `${o.dataset.nombre} (${km.toFixed(1)} km)`;

            return { opcion: o, km: km };
        });

        selectOrg.innerHTML = '';
        selectOrg.appendChild(opcionVacia);

        if (!hayOrigen) {
            opcionesOrg.forEach(o => selectOrg.appendChild(o));
        } else {
            const conKm = lista.filter(d => d.km !== null).sort((a, b) => a.km - b.km);
            const sinKm = lista.filter(d => d.km === null);

            const cercanas = conKm.filter(d => d.km <= RADIO_CERCANO_KM).map(d => d.opcion);
            const lejanas = conKm.filter(d => d.km > RADIO_CERCANO_KM).map(d => d.opcion);

            [
                crearGrupo('Cerca de ti (hasta ' + RADIO_CERCANO_KM + ' km)', cercanas),
                crearGrupo('Más lejos', lejanas),
                crearGrupo('Sin ubicación', sinKm.map(d => d.opcion))
            ]
                .filter(g => g !== null)
                .forEach(g => selectOrg.appendChild(g));
        }

        selectOrg.value = orgActual;
    }

    function actualizarEntrega() {
        const marcado = document.querySelector('input[name="Input.TipoEntrega"]:checked');
        const esDirecta = marcado && marcado.value === 'EntregaDirecta';

        seccionDireccion.hidden = esDirecta;

        const opcion = selectOrg.selectedOptions[0];
        const direccion = opcion ? opcion.dataset.direccion : '';

        infoOrg.hidden = !esDirecta && !direccion;
        infoOrg.textContent = '';

        if (!direccion) {
            infoOrg.textContent = 'Elige una organización para ver la dirección de entrega.';
            return;
        }

        infoOrg.append(esDirecta
            ? 'Llevarás tus artículos a: ' + direccion + ' '
            : 'Dirección de la organización: ' + direccion + ' ');

        const enlace = document.createElement('a');
        enlace.href = 'https://www.google.com/maps/search/?api=1&query=' + encodeURIComponent(direccion);
        enlace.target = '_blank';
        enlace.rel = 'noopener';
        enlace.textContent = 'Ver en el mapa';
        infoOrg.appendChild(enlace);
    }

    radios.forEach(r => r.addEventListener('change', actualizarEntrega));
    selectOrg.addEventListener('change', actualizarEntrega);
    selectZona.addEventListener('change', function () {
        ordenarOrganizaciones();
        actualizarEntrega();
    });

    ordenarOrganizaciones();
    actualizarEntrega();
});