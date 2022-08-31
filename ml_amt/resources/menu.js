﻿// Add settings
var g_modSettingsAMT = [];

engine.on('updateModSettingAMT', function (_name, _value) {
    for (var i = 0; i < g_modSettingsAMT.length; i++) {
        if (g_modSettingsAMT[i].name == _name) {
            g_modSettingsAMT[i].updateValue(_value);
            break;
        }
    }
});

// Modified from original `inp` types, because I have no js knowledge to hook stuff
function inp_slider_mod_amt(_obj, _callbackName) {
    this.obj = _obj;
    this.callbackName = _callbackName;
    this.minValue = parseFloat(_obj.getAttribute('data-min'));
    this.maxValue = parseFloat(_obj.getAttribute('data-max'));
    this.percent = 0;
    this.value = parseFloat(_obj.getAttribute('data-current'));
    this.dragActive = false;
    this.name = _obj.id;
    this.type = _obj.getAttribute('data-type');
    this.stepSize = _obj.getAttribute('data-stepSize') || 0;
    this.format = _obj.getAttribute('data-format') || '{value}';

    var self = this;

    if (this.stepSize != 0)
        this.value = Math.round(this.value / this.stepSize) * this.stepSize;
    else
        this.value = Math.round(this.value);

    this.valueLabelBackground = document.createElement('div');
    this.valueLabelBackground.className = 'valueLabel background';
    this.valueLabelBackground.innerHTML = this.format.replace('{value}', this.value);
    this.obj.appendChild(this.valueLabelBackground);

    this.valueBar = document.createElement('div');
    this.valueBar.className = 'valueBar';
    this.valueBar.setAttribute('style', 'width: ' + (((this.value - this.minValue) / (this.maxValue - this.minValue)) * 100) + '%;');
    this.obj.appendChild(this.valueBar);

    this.valueLabelForeground = document.createElement('div');
    this.valueLabelForeground.className = 'valueLabel foreground';
    this.valueLabelForeground.innerHTML = this.format.replace('{value}', this.value);
    this.valueLabelForeground.setAttribute('style', 'width: ' + (1.0 / ((this.value - this.minValue) / (this.maxValue - this.minValue)) * 100) + '%;');
    this.valueBar.appendChild(this.valueLabelForeground);

    this.mouseDown = function (_e) {
        self.dragActive = true;
        self.mouseMove(_e, false);
    }

    this.mouseMove = function (_e, _write) {
        if (self.dragActive) {
            var rect = _obj.getBoundingClientRect();
            var start = rect.left;
            var end = rect.right;
            self.percent = Math.min(Math.max((_e.clientX - start) / rect.width, 0), 1);
            var value = self.percent;
            value *= (self.maxValue - self.minValue);
            value += self.minValue;
            if (self.stepSize != 0) {
                value = Math.round(value / self.stepSize);
                self.value = value * self.stepSize;
                self.percent = (self.value - self.minValue) / (self.maxValue - self.minValue);
            }
            else
                self.value = Math.round(value);

            self.valueBar.setAttribute('style', 'width: ' + (self.percent * 100) + '%;');
            self.valueLabelForeground.setAttribute('style', 'width: ' + (1.0 / self.percent * 100) + '%;');
            self.valueLabelBackground.innerHTML = self.valueLabelForeground.innerHTML = self.format.replace('{value}', self.value);

            engine.call(self.callbackName, self.name, "" + self.value);
            self.displayImperial();
        }
    }

    this.mouseUp = function (_e) {
        self.mouseMove(_e, true);
        self.dragActive = false;
    }

    _obj.addEventListener('mousedown', this.mouseDown);
    document.addEventListener('mousemove', this.mouseMove);
    document.addEventListener('mouseup', this.mouseUp);

    this.getValue = function () {
        return self.value;
    }

    this.updateValue = function (value) {
        if (self.stepSize != 0)
            self.value = Math.round(value * self.stepSize) / self.stepSize;
        else
            self.value = Math.round(value);
        self.percent = (self.value - self.minValue) / (self.maxValue - self.minValue);
        self.valueBar.setAttribute('style', 'width: ' + (self.percent * 100) + '%;');
        self.valueLabelForeground.setAttribute('style', 'width: ' + (1.0 / self.percent * 100) + '%;');
        self.valueLabelBackground.innerHTML = self.valueLabelForeground.innerHTML = self.format.replace('{value}', self.value);
        self.displayImperial();
    }

    this.displayImperial = function () {
        var displays = document.querySelectorAll('.imperialDisplay');
        for (var i = 0; i < displays.length; i++) {
            var binding = displays[i].getAttribute('data-binding');
            if (binding == self.name) {
                var realFeet = ((self.value * 0.393700) / 12);
                var feet = Math.floor(realFeet);
                var inches = Math.floor((realFeet - feet) * 12);
                displays[i].innerHTML = feet + "&apos;" + inches + '&apos;&apos;';
            }
        }
    }

    return {
        name: this.name,
        value: this.getValue,
        updateValue: this.updateValue
    }
}

// Add own menu
{
    let l_block = document.createElement('div');
    l_block.innerHTML = `
        <div class ="settings-subcategory">
            <div class ="subcategory-name">Avatar Motion Tweaker</div>
            <div class ="subcategory-description"></div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Legs locomotion upright limit: </div>
            <div class ="option-input">
                <div id="CrouchLimit" class ="inp_slider no-scroll" data-min="0" data-max="100" data-current="65"></div>
            </div>
        </div>

        <div class ="row-wrapper">
            <div class ="option-caption">Legs locomotion crouch limit: </div>
            <div class ="option-input">
                <div id="ProneLimit" class ="inp_slider no-scroll" data-min="0" data-max="100" data-current="35"></div>
            </div>
        </div>
    `;
    document.getElementById('settings-implementation').appendChild(l_block);

    // Update sliders in new menu block
    let l_sliders = l_block.querySelectorAll('.inp_slider');
    for (var i = 0; i < l_sliders.length; i++) {
        g_modSettingsAMT[g_modSettingsAMT.length] = new inp_slider_mod_amt(l_sliders[i], 'MelonMod_AMT_Call_InpSlider');
    }
}
