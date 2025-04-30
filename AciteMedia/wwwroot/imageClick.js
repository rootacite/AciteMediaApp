function registerImageClick(container, dotNetObj) {
    const img = container.querySelector('img.mud-image');
    img.addEventListener('click', (e) => {
        const rect = img.getBoundingClientRect();
        const clickX = e.clientX - rect.left;
        const isLeft = clickX < rect.width / 2;
        dotNetObj.invokeMethodAsync('HandleImageClick', isLeft);
    });
}