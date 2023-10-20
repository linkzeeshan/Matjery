(function ($, ssCore, ssEx) {

    window.themeSettings = {
        themeBreakpoint: 1024,
        isAccordionMenu: false
    };

    $.fn.ForceNumericOnly = function () {
        return this.each(function () {
            $(this).keydown(function (e) {
                var key = e.charCode || e.keyCode || 0;
                // allow backspace, tab, delete, enter, arrows, numbers and keypad numbers ONLY
                // home, end, period, and numpad decimal
                return (
                    key === 8 ||
                    key === 9 ||
                    key === 13 ||
                    key === 46 ||
                    key === 110 ||
                    key === 190 ||
                    (key >= 35 && key <= 40) ||
                    (key >= 48 && key <= 57) ||
                    (key >= 96 && key <= 105));
            });
        });
    };

    function equalHeight(group) {
        var tallest = 0;

        group.each(function () {
            var thisHeight = $(this).height();
            if (thisHeight > tallest) {
                tallest = thisHeight;
            }
        });

        group.height(tallest);
    }

    function equalizeElementsHeights(themeBreakpoint) {
        if (ssCore.getViewPort().width >= themeBreakpoint) {
            //equalHeight($('.footer-box, .address-item'));
            //equalHeight($('.twocolumns>div, .order-review-data>ul'));
            equalHeight($('.sitemap-page .entity'));
            //equalHeight($(' .center-2, .side-2'));
            equalHeight($('.footer-box'));
        }
    }

    function incrementQuantityValue(event) {
        event.preventDefault();
        event.stopPropagation();

        var input = $(this).siblings('.qty-input, .productQuantityTextBox').first();

        var value = parseInt(input.val());
        if (isNaN(value)) {
            input.val(1);
            return;
        }

        value++;
        input.val(value);
    }

    function decrementQuantityValue(event) {
        event.preventDefault();
        event.stopPropagation();

        var input = $(this).siblings('.qty-input, .productQuantityTextBox').first();

        var value = parseInt(input.val());

        if (isNaN(value)) {
            input.val(1);
            return;
        }

        if (value <= 1) {
            return;
        }

        value--;
        input.val(value);
    }

    function headerLinksClickable(anchor, target) {
        $(anchor).on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            if ($(target).hasClass('active')) {
                $(target + ', ' + anchor).removeClass('active');
            } else {
                $(target + ', ' + anchor).siblings().removeClass('active');
                $(target + ', ' + anchor + ', .header-links-wrapper').addClass('active');
            }
        });
    }
    
    // Custom select elements
    function customSelect() {
        $('.header-selectors-wrapper select, .urban-theme-roller select').each(function () {
            var that = $(this);

            that.wrap('<div class="custom-select" />');
            $('<div class="custom-select-text" />').prependTo(that.parent('.custom-select'));
            that.siblings('.custom-select-text').text(that.children('option:selected').text());
        }).change(function () {
            $(this).siblings('.custom-select-text').text($(this).children('option:selected').text());
        });
    }

    // Home Page Slider
    function homePageSliderUnderHeader() {
        if ($('.home_page_main_slider').length > 0) {
            $('.master-wrapper-page.home-page.header-inside-slider .header').addClass('becomeAbsolute');
        }
    }

    $(document).ready(function () {
        var responsiveAppSettings = {
            isEnabled: true,
            themeBreakpoint: window.themeSettings.themeBreakpoint,
            isSearchBoxDetachable: false,
            isHeaderLinksWrapperDetachable: false,
            doesDesktopHeaderMenuStick: false,
            doesScrollAfterFiltration: true,
            doesSublistHasIndent: true,
            displayGoToTop: true,
            hasStickyNav: false,
            selectors: {
                menuTitle: ".header-menu-opener",
                headerMenu: ".header-menu",
                closeMenu: ".close-menu",
                movedElements: ".admin-header-links, .header, .responsive-nav-wrapper, .master-wrapper-content, .footer",
                sublist: ".header-menu .sublist",
                overlayOffCanvas: ".overlayOffCanvas",
                withSubcategories: ".with-subcategories",
                filtersContainer: ".nopAjaxFilters7Spikes",
                filtersOpener: ".filters-button span",
                searchBoxOpener: ".search-wrap > span",
                searchBox: ".search-box",
                searchBoxBefore: ".header-logo",
                navWrapper: ".responsive-nav-wrapper",
                navWrapperParent: ".responsive-nav-wrapper-parent",
                headerLinksOpener: "#header-links-opener",
                headerLinksWrapper: ".header-links-wrapper",
                shoppingCartLink: ""
            }
        };

        ssEx.initResponsiveTheme(responsiveAppSettings);

        // CUSTOM SELECTS
        $(".product-selectors select").simpleSelect();

        // Header links
        $('.shopping-cart-opener a.ico-cart').on('click', function (e) { e.preventDefault(); });

        headerLinksClickable('.shopping-cart-opener', '.header-links-wrapper .shopping-cart-wrapper');
        headerLinksClickable('.user-links-opener', '.header-links-wrapper .user-links-wrapper');
        headerLinksClickable('.search-opener', '.header-links-wrapper .search-box-wrapper');
        headerLinksClickable('.header-selectors-opener', '.header-links-wrapper .header-selectors-wrapper');

        // Set white background to header strip in mobile layout
        $('.home-page.header-inside-slider .header.mobile .header-strip li').on('click', function () {
            if ($('.home-page.header-inside-slider .header.mobile .header-strip li.active').length > 0) {
                $('.header-strip').addClass('white-background');
            } else {
                $('.header-strip').removeClass('white-background');
            }
        });

        // Numeric only 
        $('.add-to-cart-panel .qty-input, .productQuantityTextBox').ForceNumericOnly();

        // Call select customisation function
        if (ssCore.getViewPort().width >= responsiveAppSettings.themeBreakpoint) {
            customSelect();
        }

        // Custom scroll
        $('.header.mobile .fullWidth, .header.mobile .header-menu, .header.mobile .mega-menu-responsive ').perfectScrollbar({
            swipePropagation: false,
            wheelSpeed: 1,
            suppressScrollX: true
        });

        // Make social icons 2 in line if even
        var socIcons = $('.social-buttons li');
        if (socIcons.length % 2 === 0 && socIcons.length < 5) {
            socIcons.addClass('reshuffle');
        }

        // Toggle filters
        $(".horizontalFiltersTitle").click(function () {
            $(this).toggleClass('opened');
            $(".onecolumn .horizontalFiltersCollapsibleContent").slideToggle("slow", function () {
                $(this).css('overflow', '');
            });
        });

        // Case when category banner is only one
        if ($('.banner-included .slider-wrapper').length === 1) {
            $('.home-page-category-grid.banner-included .item-box:nth-child(n+4)').addClass('oneBannerOnly');
            $('.home-page-category-grid.banner-included .slider-wrapper').parents('.item-grid').addClass('oneBannerIncluded');
            $('.oneBannerOnly').wrapAll('<div class="clearingWrapper"></div>');
        }

        // Plus, Minus Quantity buttons
        // Normal & Ajax Cart
        $(document).on('click', '.item-box .plus', incrementQuantityValue).on('click', '.item-box .minus', decrementQuantityValue);

        $('.ajaxCart').on('click', '.plus', incrementQuantityValue).on('click', '.minus', decrementQuantityValue);
        $('.variant-overview').on('click', '.plus', incrementQuantityValue).on('click', '.minus', decrementQuantityValue);

        // Home Page Categories Tabs Normal & Ajax Cart
        $('.home-page-category-tabs').on('click', '.plus', incrementQuantityValue).on('click', '.minus', decrementQuantityValue);

        // Quick View
        $('.quickView').on('click', '.plus', incrementQuantityValue).on('click', '.minus', decrementQuantityValue);

        // product Details Page
        $('.product-essential .plus').on('click', incrementQuantityValue);
        $('.product-essential .minus').on('click', decrementQuantityValue);

        // Equal height elements
        equalizeElementsHeights(responsiveAppSettings.themeBreakpoint);
        ssEx.addWindowEvent("resize", function () { equalizeElementsHeights(responsiveAppSettings.themeBreakpoint); });
        ssEx.addWindowEvent("orientationchange", function () { equalizeElementsHeights(responsiveAppSettings.themeBreakpoint); });

        // Home Page Slider Position
        homePageSliderUnderHeader();

        // Close header links on click outside 
        $('html').click(function () {
            $('.header-links-wrapper, .header-links-wrapper > div').removeClass('active');
            $('.header-inside-slider .header-strip').removeClass('white-background');
        });

        $('.header-strip').click(function (event) {
            event.stopPropagation();
        });

        // Footer footer-box
        if ($('.footer-box').length === 2) {
            $('.footer-box').css('width', '50%');
        }
    });
})(jQuery, sevenSpikesCore, sevenSpikesEx);