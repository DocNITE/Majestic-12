/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */


using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.KayMisaZlevels.Server.Components;

/// <summary>
/// This is used for loading prebuilted maps
/// </summary>
[RegisterComponent]
public sealed partial class ZDefinedStackComponent : Component
{
    /// <summary>
    /// A map paths to load on a new map lower.
    /// </summary>
    [DataField("downLevels")]
    public List<ResPath> DownLevels = new();

    /// <summary>
    /// A map paths to load on a new map up.
    /// </summary>
    [DataField("upLevels")]
    public List<ResPath> UpLevels = new();
}


//TODO:
/*

Потом обязательно нужен сделать темплейт уровень

Затем нужно починить тайм цикл. Сделать так чтобы они работал с другими компонентами, как бы связывая несколько карт в одну систему

Для создания 2D шейдера в Godot, который затемняет объект на определенный процент, необходимо использовать язык шейдеров Godot Shader Language (GDScript). Вот пример простого шейдера, который затемняет цвет пикселей:


------------------------------------
shader_type canvas_item;

uniform float darkness : hint_range(0.0, 1.0) = 0.2; // Процент затемнения (0.0 - 1.0)

void fragment() {
    vec4 texColor = texture(TEXTURE, UV);
    // Затемняем цвет
    texColor.rgb *= (1.0 - darkness);
    COLOR = texColor;
}
-----------------------------------

И с блюром

-----------------------------------------
shader_type canvas_item;

uniform float darken_strength : hint_range(0.0, 1.0) = 0.3; // Степень затемнения
uniform float blur_radius : hint_range(0.0, 10.0) = 1.0; // Радиус размытия

void fragment() {
    // Получаем цвет пикселя
    vec4 color = texture(TEXTURE, UV);

    // Затемнение цвета
    color.rgb *= (1.0 - darken_strength);

    // Применяем блюр
    vec4 blurred_color = vec4(0.0);
    for (int x = -1; x <= 1; x++) {
        for (int y = -1; y <= 1; y++) {
            blurred_color += texture(TEXTURE, UV + vec2(x, y) * blur_radius / SCREEN_SIZE);
        }
    }
    blurred_color /= 9.0; // Делаем среднее из соседних пикселей

    // Устанавливаем финальный цвет
    COLOR = mix(color, blurred_color, 0.5);
}
-----------------------------------------


### Объяснение кода:

1. `shader_type canvas_item;` - Определяет, что данный шейдер будет использоваться для 2D графики.

2. `uniform float darkness : hint_range(0.0, 1.0) = 0.2;` - Создает пользовательскую переменную, которая позволяет вам устанавливать уровень затемнения от 0.0 (без затемнения) до 1.0 (полная чернота). По умолчанию установлено значение 0.2 (20% затемнения).

3. `void fragment() { ... }` - Функция, которая вызывается для каждого пикселя. Она берет цвет текстуры и затемняет его.

4. `texColor.rgb *= (1.0 - darkness);` - Умножает цвет на значение, уменьшенное на процент затемнения, тем самым создавая эффект затемнения.

### Как использовать этот шейдер:

1. Создайте новый материал в Godot.
2. Установите тип материала на "ShaderMaterial".
3. Присоедините к нему созданный шейдер.
4. Примените материал к нужному узлу 2D (например, Sprite).

Теперь вы сможете регулировать процент затемнения через параметр `darkness` в инспекторе.

*/
