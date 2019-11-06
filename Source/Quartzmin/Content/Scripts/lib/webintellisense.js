var wi;
(function (wi) {
    (function (Keys) {
        Keys[Keys["Backspace"] = 8] = "Backspace";
        Keys[Keys["Tab"] = 9] = "Tab";
        Keys[Keys["Enter"] = 13] = "Enter";
        Keys[Keys["Escape"] = 27] = "Escape";
        Keys[Keys["PageUp"] = 33] = "PageUp";
        Keys[Keys["PageDown"] = 34] = "PageDown";
        Keys[Keys["Left"] = 37] = "Left";
        Keys[Keys["Up"] = 38] = "Up";
        Keys[Keys["Right"] = 39] = "Right";
        Keys[Keys["Down"] = 40] = "Down";
    })(wi.Keys || (wi.Keys = {}));
    var Keys = wi.Keys;
    /**
     * Shows or hides an element by setting the display style to 'block' for true
     * or 'none' for false.
     * @param element - The element to show or hide
     * @param b - To show or hide the element
     */
    function showElement(el, b) {
        el.style.display = b ? 'block' : 'none';
    }
    wi.showElement = showElement;
    /**
     * Checks to see if an element has a CSS class
     * @param element - The element to add the class
     */
    function hasCssClass(el, name) {
        var classes = el.className.split(/\s+/g);
        return classes.indexOf(name) !== -1;
    }
    wi.hasCssClass = hasCssClass;
    /**
     * Adds a CSS class from an element
     * @param element - The element to add the class
     * @param name - The name of the class to add
     */
    function addCssClass(el, name) {
        if (!hasCssClass(el, name)) {
            el.className += " " + name;
        }
    }
    wi.addCssClass = addCssClass;
    /**
     * Removes a CSS class from an element
     * @param element - The element to remove the class
     * @param name - The name of the class to remove
     */
    function removeCssClass(el, name) {
        var classes = el.className.split(/\s+/g);
        while (true) {
            var index = classes.indexOf(name);
            if (index === -1) {
                break;
            }
            classes.splice(index, 1);
        }
        el.className = classes.join(" ");
    }
    wi.removeCssClass = removeCssClass;
    /**
     * Looks for the last index of a number of strings inside of another string
     * @param str - The string to search within
     * @param arr - An array of strings to search for
     * @param [start] - Optional starting position
     */
    function lastIndexOfAny(str, arr, start) {
        var max = -1;
        for (var i = 0; i < arr.length; i++) {
            var val = str.lastIndexOf(arr[i], start);
            max = Math.max(max, val);
        }
        return max;
    }
    wi.lastIndexOfAny = lastIndexOfAny;
    /**
     * An item that is displayed within the declarations user interface.
     */
    var DeclarationItem = (function () {
        function DeclarationItem() {
        }
        return DeclarationItem;
    }());
    wi.DeclarationItem = DeclarationItem;
    /**
     * Provides a user interface for a tooltip.
     */
    var Tooltip = (function () {
        function Tooltip() {
            this.visible = false;
            this.events = { visibleChanged: [] };
            this.tooltipElement = document.getElementById('br-tooltip-div');
            if (this.tooltipElement == null) {
                this.tooltipElement = document.createElement('div');
                this.tooltipElement.id = 'br-tooltip-div';
                this.tooltipElement.className = 'br-tooltip';
                document.body.appendChild(this.tooltipElement);
            }
        }
        /**
         * Triggers the visible changed callback events
         */
        Tooltip.prototype.triggerVisibleChanged = function () {
            var _this = this;
            this.events.visibleChanged.forEach(function (callback) {
                callback(_this.visible);
            });
        };
        /**
         * Check to see if the user interface is visible or not
         * @returns True if visible otherwise false
         */
        Tooltip.prototype.isVisible = function () {
            return this.visible;
        };
        /**
         * Sets the visibility of the tooltip element
         * @param b True to set visible, false to hide
         */
        Tooltip.prototype.setVisible = function (b) {
            if (this.visible !== b) {
                this.visible = b;
                showElement(this.tooltipElement, b);
                this.triggerVisibleChanged();
            }
        };
        /**
         * Sets the HTML of the tooltip element
         * @param html The html to set
         */
        Tooltip.prototype.setHtml = function (html) {
            this.tooltipElement.innerHTML = html;
        };
        /**
         * Sets the text of the tooltip element
         * @param text The text to set
         */
        Tooltip.prototype.setText = function (text) {
            this.tooltipElement.innerText = text;
        };
        /**
         * Gets the inner text of the tooltip element
         * @returns The inner text of the element
         */
        Tooltip.prototype.getText = function () {
            return this.tooltipElement.innerText;
        };
        /**
         * Gets the inner html of the tooltip elemnt
         * @returns The inner HTML
         */
        Tooltip.prototype.getHtml = function () {
            return this.tooltipElement.innerHTML;
        };
        /**
         * Sets the position on screen of the tooltip element
         * @param left The left pixel position
         * @param top The top pixel position
         */
        Tooltip.prototype.setPosition = function (left, top) {
            this.tooltipElement.style.left = left + 'px';
            this.tooltipElement.style.top = top + 'px';
        };
        return Tooltip;
    }());
    wi.Tooltip = Tooltip;
    /**
     * Provides a user interface for a methods popup. This class basically generates
     * a div that preview a list of strings.
     */
    var MethodsIntellisense = (function () {
        function MethodsIntellisense() {
            var _this = this;
            this.events = { visibleChanged: [] };
            this.visible = false;
            this.methods = [];
            this.selectedIndex = 0;
            this.methodsElement = document.createElement('div');
            this.methodsTextElement = document.createElement('div');
            this.arrowsElement = document.createElement('div');
            this.upArrowElement = document.createElement('span');
            this.downArrowElement = document.createElement('span');
            this.arrowTextElement = document.createElement('span');
            this.methodsElement.className = 'br-methods';
            this.methodsTextElement.className = 'br-methods-text';
            this.arrowsElement.className = 'br-methods-arrows';
            this.upArrowElement.className = 'br-methods-arrow';
            this.upArrowElement.innerHTML = '&#8593;';
            this.downArrowElement.className = 'br-methods-arrow';
            this.downArrowElement.innerHTML = '&#8595;';
            this.arrowTextElement.className = 'br-methods-arrow-text';
            this.arrowsElement.appendChild(this.upArrowElement);
            this.arrowsElement.appendChild(this.arrowTextElement);
            this.arrowsElement.appendChild(this.downArrowElement);
            this.methodsElement.appendChild(this.arrowsElement);
            this.methodsElement.appendChild(this.methodsTextElement);
            document.body.appendChild(this.methodsElement);
            // arrow click events
            this.downArrowElement.onclick = function () {
                _this.moveSelected(1);
            };
            // arrow click events
            this.upArrowElement.onclick = function () {
                _this.moveSelected(-1);
            };
        }
        /**
         * Sets the selected item by index. Wrapping is performed if the index
         * specified is out of bounds of the methods that are set.
         * @param idx The index of the item to set selected
         */
        MethodsIntellisense.prototype.setSelectedIndex = function (idx) {
            if (idx < 0) {
                idx = this.methods.length - 1;
            }
            else if (idx >= this.methods.length) {
                idx = 0;
            }
            this.selectedIndex = idx;
            this.methodsTextElement.innerHTML = this.methods[idx];
            this.arrowTextElement.innerHTML = (idx + 1) + ' of ' + this.methods.length;
        };
        /**
         * Sets the methods to display. If not empty, the user interface is shown and the
         * first methods is selected.
         * @param methods The methods to populate the interface with
         */
        MethodsIntellisense.prototype.setMethods = function (data) {
            if (data != null && data.length > 0) {
                this.methods = data;
                // show the elements
                this.setVisible(true);
                // show the first item
                this.setSelectedIndex(0);
            }
        };
        /**
         * Sets the position of the UI element.
         * @param left The left position
         * @param top The top position
         */
        MethodsIntellisense.prototype.setPosition = function (left, top) {
            this.methodsElement.style.left = left + 'px';
            this.methodsElement.style.top = top + 'px';
        };
        /**
         * Sets the currently selected index by delta.
         * @param delta The amount to move
         */
        MethodsIntellisense.prototype.moveSelected = function (delta) {
            this.setSelectedIndex(this.selectedIndex + delta);
        };
        /**
         * Checks to see if the UI is visible
         * @returns True if visible, otherwise false
         */
        MethodsIntellisense.prototype.isVisible = function () {
            return this.visible;
        };
        /**
         * Shows or hides the UI
         * @param b Show or hide the user interface
         */
        MethodsIntellisense.prototype.setVisible = function (b) {
            if (this.visible !== b) {
                this.visible = b;
                showElement(this.methodsElement, b);
                this.triggerVisibleChanged();
            }
        };
        MethodsIntellisense.prototype.triggerVisibleChanged = function () {
            var _this = this;
            this.events.visibleChanged.forEach(function (callback) {
                callback(_this.visible);
            });
        };
        /**
         * Provides common keyboard event handling for a keydown event.
         *
         * escape, left, right -> hide the UI
         * up -> select previous item
         * down -> select next item
         * pageup -> select previous 5th
         * pagedown -> select next 5th
         *
         * @param evt The event
         */
        MethodsIntellisense.prototype.handleKeyDown = function (evt) {
            if (evt.keyCode === Keys.Escape || evt.keyCode === Keys.Left || evt.keyCode === Keys.Right) {
                this.setVisible(false);
            }
            else if (evt.keyCode === Keys.Up) {
                this.moveSelected(-1);
                evt.preventDefault();
                evt.stopPropagation();
            }
            else if (evt.keyCode === Keys.Down) {
                this.moveSelected(1);
                evt.preventDefault();
                evt.stopPropagation();
            }
            else if (evt.keyCode === Keys.PageUp) {
                this.moveSelected(-5);
                evt.preventDefault();
            }
            else if (evt.keyCode === Keys.PageDown) {
                this.moveSelected(5);
                evt.preventDefault();
            }
        };
        /**
         * Adds an event listener for the `onVisibleChanged` event
         * @param callback The callback to add
         */
        MethodsIntellisense.prototype.onVisibleChanged = function (callback) {
            this.events.visibleChanged.push(callback);
        };
        return MethodsIntellisense;
    }());
    wi.MethodsIntellisense = MethodsIntellisense;
    /**
     * Provides a user interface for a declarations popup. This class basically
     * generates a div that acts as a list of items. When items are displayed (usually
     * triggered by a keyboard event), the user can select an item from the list.
     */
    var DeclarationsIntellisense = (function () {
        function DeclarationsIntellisense() {
            this.events = { itemChosen: [], itemSelected: [], visibleChanged: [] };
            this.selectedIndex = 0;
            this.filteredDeclarations = [];
            this.filteredDeclarationsUI = [];
            this.visible = false;
            this.declarations = [];
            this.filterText = '';
            this.filterModes = {
                startsWith: function (item, filterText) {
                    return item.name.toLowerCase().indexOf(filterText) === 0;
                },
                contains: function (item, filterText) {
                    return item.name.toLowerCase().indexOf(filterText) >= 0;
                }
            };
            this.filterMode = this.filterModes.startsWith;
            // ui widgets
            this.selectedElement = null;
            this.listElement = document.createElement('ul');
            this.documentationElement = document.createElement('div');
            this.listElement.className = 'br-intellisense';
            this.documentationElement.className = 'br-documentation';
            document.body.appendChild(this.listElement);
            document.body.appendChild(this.documentationElement);
        }
        /**
         * Provides common keyboard event handling for a keydown event.
         *
         * escape, left, right -> hide the UI
         * up -> select previous item
         * down -> select next item
         * pageup -> select previous 5th
         * pagedown -> select next 5th
         * enter, tab -> chooses the currently selected item
         *
         * @param evt The event
         */
        DeclarationsIntellisense.prototype.handleKeyDown = function (evt) {
            if (evt.keyCode == Keys.Escape) {
                this.setVisible(false);
                evt.preventDefault();
                evt.cancelBubble = true;
            }
            else if (evt.keyCode === Keys.Left || evt.keyCode === Keys.Right || evt.keyCode === Keys.Escape) {
                this.setVisible(false);
            }
            else if (evt.keyCode === Keys.Up) {
                this.moveSelected(-1);
                evt.preventDefault();
                evt.cancelBubble = true;
            }
            else if (evt.keyCode === Keys.Down) {
                this.moveSelected(1);
                evt.preventDefault();
                evt.cancelBubble = true;
            }
            else if (evt.keyCode === Keys.PageUp) {
                this.moveSelected(-5);
                evt.preventDefault();
                evt.cancelBubble = true;
            }
            else if (evt.keyCode === Keys.PageDown) {
                this.moveSelected(5);
                evt.preventDefault();
                evt.cancelBubble = true;
            }
            else if (evt.keyCode === Keys.Enter || evt.keyCode === Keys.Tab) {
                this.triggerItemChosen(this.getSelectedItem());
                evt.preventDefault();
                evt.cancelBubble = true;
            }
        };
        DeclarationsIntellisense.prototype.triggerVisibleChanged = function () {
            var _this = this;
            this.events.visibleChanged.forEach(function (callback) {
                callback(_this.visible);
            });
        };
        DeclarationsIntellisense.prototype.triggerItemChosen = function (item) {
            this.events.itemChosen.forEach(function (callback) {
                callback(item);
            });
        };
        DeclarationsIntellisense.prototype.triggerItemSelected = function (item) {
            this.events.itemSelected.forEach(function (callback) {
                callback(item);
            });
        };
        /**
         * Gets the currently selected index
         */
        DeclarationsIntellisense.prototype.getSelectedIndex = function () {
            return this.selectedIndex;
        };
        /**
         * Sets the currently selected index
         * @param idx The index to set
         */
        DeclarationsIntellisense.prototype.setSelectedIndex = function (idx) {
            if (idx !== this.selectedIndex) {
                this.selectedIndex = idx;
                this.triggerItemSelected(this.getSelectedItem());
            }
        };
        /**
         * Adds an event listener for the `onItemChosen` event
         * @param callback The callback to call when an item is chosen
         */
        DeclarationsIntellisense.prototype.onItemChosen = function (callback) {
            this.events.itemChosen.push(callback);
        };
        /**
         * Adds an event listener for the `onItemSelected` event
         * @param callback The callback to call when an item is selected
         */
        DeclarationsIntellisense.prototype.onItemSelected = function (callback) {
            this.events.itemSelected.push(callback);
        };
        /**
         * Adds an event listener for the `onVisibleChanged` event
         * @param callback The callback to call when the ui is shown or hidden
         */
        DeclarationsIntellisense.prototype.onVisibleChanged = function (callback) {
            this.events.visibleChanged.push(callback);
        };
        /**
         * Gets the selected item
         */
        DeclarationsIntellisense.prototype.getSelectedItem = function () {
            return this.filteredDeclarations[this.selectedIndex];
        };
        DeclarationsIntellisense.prototype.createListItemDefault = function (item) {
            var listItem = document.createElement('li');
            listItem.innerHTML = '<span class="br-icon icon-glyph-' + item.glyph + '"></span> ' + item.name;
            listItem.className = 'br-listlink';
            return listItem;
        };
        DeclarationsIntellisense.prototype.refreshSelected = function () {
            if (this.selectedElement != null) {
                removeCssClass(this.selectedElement, 'br-selected');
            }
            this.selectedElement = this.filteredDeclarationsUI[this.selectedIndex];
            if (this.selectedElement) {
                addCssClass(this.selectedElement, 'br-selected');
                var item = this.getSelectedItem();
                if (item.documentation == null) {
                    this.showDocumentation(false);
                }
                else {
                    this.showDocumentation(true);
                    this.documentationElement.innerHTML = item.documentation;
                }
                var top = this.selectedElement.offsetTop;
                var bottom = top + this.selectedElement.offsetHeight;
                var scrollTop = this.listElement.scrollTop;
                if (top <= scrollTop) {
                    this.listElement.scrollTop = top;
                }
                else if (bottom >= scrollTop + this.listElement.offsetHeight) {
                    this.listElement.scrollTop = bottom - this.listElement.offsetHeight;
                }
            }
        };
        DeclarationsIntellisense.prototype.refreshUI = function () {
            var _this = this;
            this.listElement.innerHTML = '';
            this.filteredDeclarationsUI = [];
            this.filteredDeclarations.forEach(function (item, idx) {
                var listItem = _this.createListItemDefault(item);
                listItem.ondblclick = function () {
                    _this.setSelectedIndex(idx);
                    _this.triggerItemChosen(_this.getSelectedItem());
                    _this.setVisible(false);
                    _this.showDocumentation(false);
                };
                listItem.onclick = function () {
                    _this.setSelectedIndex(idx);
                };
                _this.listElement.appendChild(listItem);
                _this.filteredDeclarationsUI.push(listItem);
            });
            this.refreshSelected();
        };
        DeclarationsIntellisense.prototype.showDocumentation = function (b) {
            showElement(this.documentationElement, b);
        };
        /**
         * Checks to see if the UI is visible
         */
        DeclarationsIntellisense.prototype.setVisible = function (b) {
            if (this.visible !== b) {
                this.visible = b;
                showElement(this.listElement, b);
                showElement(this.documentationElement, b);
                this.triggerVisibleChanged();
            }
        };
        /**
         * Sets the declarations to display. If not empty, the user interface is shown and the
         * first item is selected.
         * @param data The array of declaration items to show
         */
        DeclarationsIntellisense.prototype.setDeclarations = function (data) {
            if (data != null && data.length > 0) {
                // set the data
                this.declarations = data;
                this.filteredDeclarations = data;
                // show the elements
                this.setSelectedIndex(0);
                this.setFilter('');
                this.setVisible(true);
                this.refreshUI();
            }
        };
        /**
         * Sets the position of the UI element.
         * @param left The left position
         * @param top The top position
         */
        DeclarationsIntellisense.prototype.setPosition = function (left, top) {
            // reposition intellisense
            this.listElement.style.left = left + 'px';
            this.listElement.style.top = top + 'px';
            // reposition documentation (magic number offsets can't figure out why)
            this.documentationElement.style.left = (left + this.listElement.offsetWidth + 5) + 'px';
            this.documentationElement.style.top = (top + 5) + 'px';
        };
        /**
         * Setter for how the filter behaves. There are two default implementations
         * startsWith and contains.
         *
         * The `startsWith` mode checks that the `name` property
         * of the item starts with the filter text
         *
         * The `contains` mode checks for any
         * substring of the filter text in the `name` property of the item.
         *
         * @param mode The mode to set
         */
        DeclarationsIntellisense.prototype.setFilterMode = function (mode) {
            if (typeof (mode) === 'function') {
                this.filterMode = mode;
            }
            else if (typeof (mode) === 'string') {
                this.filterMode = this.filterModes[mode];
            }
        };
        /**
         * Setter for the filter text. When set, the items displayed are
         * automatically filtered
         *
         * @param f The filter to set
         */
        DeclarationsIntellisense.prototype.setFilter = function (f) {
            var _this = this;
            if (this.filterText !== f) {
                this.setSelectedIndex(0);
                this.filterText = f;
            }
            var ret = [];
            this.declarations.forEach(function (item) {
                if (_this.filterMode(item, _this.filterText)) {
                    ret.push(item);
                }
            });
            this.filteredDeclarations = ret;
            this.refreshUI();
        };
        /**
         * Sets the currently selected index by delta.
         * @param delta The number of items to move
         */
        DeclarationsIntellisense.prototype.moveSelected = function (delta) {
            var idx = this.selectedIndex + delta;
            idx = Math.max(idx, 0);
            idx = Math.min(idx, this.filteredDeclarations.length - 1);
            // select
            this.setSelectedIndex(idx);
            this.refreshSelected();
        };
        /**
         * Check to see if the declarations div is visible
         */
        DeclarationsIntellisense.prototype.isVisible = function () {
            return this.visible;
        };
        return DeclarationsIntellisense;
    }());
    wi.DeclarationsIntellisense = DeclarationsIntellisense;
})(wi || (wi = {}));
