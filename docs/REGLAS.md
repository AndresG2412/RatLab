# Reglas del juego

- Cada carta tiene 4 valores: Norte, Este, Sur y Oeste, cada uno de 1 a 9.
- El tablero es de 3x3 (9 casillas).
- Jugador y enemigo reciben 5 cartas cada uno. Empieza el jugador, así que el jugador juega 5 cartas y el enemigo 4; una carta del enemigo queda sin usar.
- Empieza el jugador. Luego se alternan los turnos.
- Al colocar una carta, se compara cada uno de sus lados con el lado opuesto de la carta vecina: mi Norte contra el Sur de la de arriba, mi Este contra el Oeste de la de la derecha, mi Sur contra el Norte de la de abajo, mi Oeste contra el Este de la de la izquierda.
- Si mi número es estrictamente mayor y la carta vecina es del rival, esa carta cambia de dueño y de color. Si hay empate o es menor, no pasa nada.
- Solo ataca la carta recién colocada. Las cartas ya puestas no atacan.
- Las casillas vacías no se comparan.
- Al llenarse el tablero, gana quien tenga más cartas de su color. Si hay igualdad, es empate.
- Colores por dueño: azul = jugador, rojo = enemigo, gris = casilla vacía.
- Por ahora las cartas se generan con valores aleatorios de 1 a 9.
