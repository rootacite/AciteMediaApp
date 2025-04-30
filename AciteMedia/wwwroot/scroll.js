
function getScrollPosition(selector) {
    const container = document.querySelector(selector);
    return container.scrollLeft;
}

function setScrollPosition(selector, position) {
    const container = document.querySelector(selector);
    container.scrollLeft = position;
}

function setupScrollListener(selector, dotnetHelper) {
    const container = document.querySelector(selector);
    container.addEventListener("scroll", () => {
        const scrollLeft = container.scrollLeft;
        dotnetHelper.invokeMethodAsync("OnScroll", scrollLeft);
    });
}

function getMaxScrollPosition(selector) {
    const container = document.querySelector(selector);
    return container.scrollWidth - container.clientWidth;
}