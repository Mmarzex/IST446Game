var colors = require('colors'),
    _ = require('lodash'),
    fs = require('fs');

var Tile = require('./tile'),
    Rect = require('./rect');

// Map properties
var MAX_WIDTH = 50,
    MAX_HEIGHT = 50,
    MAX_ROOM_SIZE = 10,
    MIN_ROOM_SIZE = 6,
    MAX_ROOMS = 15,
    MIN_ROOMS = 5,
    MIN_ENEMIES_PER_ROOM = 5,
    MAX_ENEMIES_PER_ROOM = 10;

// Terrain values
var GROUND = 0,
    WALL = 1;

// Enemy types
var ENEMY_TYPES = {
    // Temp enemy names
    ENEMY_1: 0,
    ENEMY_2: 1,
    ENEMY_3: 2
};

var Map = function() {
    this.terrain = [];
    this.rooms = [];
    this.enemies = [];
    this.playerSpawn = {};
    this.treasurePosition = {};
    this.properties = {
        width: MAX_WIDTH,
        height: MAX_HEIGHT
    };

    // Start with a terrain that is all walls, then start carving rooms and tunnels
    for (var x = 0; x < MAX_WIDTH; x++) {
        this.terrain[x] = [];
        for (var y = 0; y < MAX_HEIGHT; y++) {
            this.terrain[x][y] = WALL;
        }
    }

    this.initTerrain();
    this.generateExitDoor();
};

/**
 * Generates the map's terrain array
 */
Map.prototype.initTerrain = function() {
    var roomCount = getRandomInt(MIN_ROOMS, MAX_ROOMS),
        prevX = 0,
        prevY = 0;

    for (var r = 0; r < roomCount; r++) {

        // Keep generating rooms until a valid one is found
        var isValidRoom = false,
            newRoom;
        while (!isValidRoom) {
            newRoom = this.generateRoom();
            isValidRoom = true;
            for (var i = 0; i < this.rooms.length; i++) {
                if (newRoom.intersects(this.rooms[i])) {
                    isValidRoom = false;
                    i = this.rooms.length;
                }
            }
        }

        if (isValidRoom) {
            this.createRoom(newRoom);

            // Don't add enemies to the room that the player spawns in
            if (r > 0) {
                this.generateEnemies(newRoom);
            }

            // Connect to previously created room (if there is a previous room)
            if (this.rooms.length == 0) {
                // Make first room the room that the player starts in
                this.playerSpawn = newRoom.center();
            } else {

                var prevRoom = this.rooms[this.rooms.length - 1];
                console.log(newRoom.center());
                console.log(prevRoom.center());

                var hv = Math.round(Math.random());
                if (hv === 1) {
                    this.createHTunnel(prevRoom.center().y, newRoom.center().y, prevRoom.center().x);
                    // Add extra horizontal tunnel to vary widths of tunnnels, and add tunnels that leads to a dead end
                    // Due to varyng widts of the tunnels, causes rooms not to be perfect rectangles
                    this.createHTunnel(prevRoom.center().y, newRoom.center().y, prevRoom.center().x + 1);
                    this.createVTunnel(prevRoom.center().x, newRoom.center().x, newRoom.center().y);
                } else {
                    this.createVTunnel(prevRoom.center().x, newRoom.center().x, prevRoom.center().y);
                    this.createHTunnel(prevRoom.center().y, newRoom.center().y, newRoom.center().x);
                    this.createHTunnel(prevRoom.center().y, newRoom.center().y, prevRoom.center().x + 1);
                }
            }
            this.rooms.push(newRoom);

        }
    }

    var randRoom = getRandomInt(0, this.rooms.length - 1);
    var treasureRoom = this.rooms[randRoom];

    // Add treasure to any room that is not the start room
    this.treasurePosition = {
        x: getRandomArbitrary(treasureRoom.x1 + 1, treasureRoom.x2 - 1),
        y: getRandomArbitrary(treasureRoom.y1 + 1, treasureRoom.y2 - 1)
    };

    console.log('Generated trasure at: ' + this.treasurePosition.x + ',' + this.treasurePosition.y);

};

/**
 * Creates a room given a Rect object
 * @param {Rect} room - Rect object with start x, y, width, and height
 */
Map.prototype.createRoom = function(room) {
    console.log('Creating room');
    console.log(room);
    for (var x = room.x1; x < room.x2; x++) {
        for (var y = room.y1; y < room.y2; y++) {
            this.terrain[x][y] = GROUND;
        }
    }
};

/**
 * Creates a horizontal tunnel at a coordinate
 * @param  {int} x1 - Start x position
 * @param  {int} x2 - end x position
 * @param  {y} y - start y position
 */
Map.prototype.createHTunnel = function(x1, x2, y) {
    var start = Math.min(x1, x2),
        end = Math.max(x1, x2);

    console.log('creating horizontal tunnel [' + start + ', ' + y + '] -> [' + end + ', ' + y + ']');
    for (var x = start; x < end + 1; x++) {
        this.terrain[y][x] = GROUND;
    }
};


/**
 * Creates a vertical tunnel at a coordinate
 * @param  {int} y1 - Start y position
 * @param  {int} y2 - Ending y position
 * @param  {x} - Width of tunnel
 */
Map.prototype.createVTunnel = function(y1, y2, x) {
    var start = Math.min(y1, y2),
        end = Math.max(y1, y2);

    console.log('creating vertical tunnel ' + start + ' -> ' + end + ' [' + x + ',' + y1 + ']');
    for (var y = start; y < end; y++) {
        this.terrain[y][x] = GROUND;
    }
};

/**
 * Generates a room in a random position of random size.
 * @return {Rect}
 */
Map.prototype.generateRoom = function() {
    var width = Math.floor(Math.random() * (MAX_ROOM_SIZE - MIN_ROOM_SIZE) + MIN_ROOM_SIZE),
        height = Math.floor(Math.random() * (MAX_ROOM_SIZE - MIN_ROOM_SIZE) + MIN_ROOM_SIZE),
        positionX = Math.floor(Math.random() * (MAX_WIDTH - width)),
        positionY = Math.floor(Math.random() * (MAX_HEIGHT - height));
    console.log('Creating room of size: ' + width + 'x' + height + ' at [' + positionX + ',' + positionY + ']');

    var newRoom = new Rect(positionX, positionY, width, height);

    return newRoom;
};

/**
 * Generates enemies for a given room
 * @param  {object} room - Room to add enemies to
 */
Map.prototype.generateEnemies = function(room) {
    // Generate enemies for room
    var generatedEnemies = 0,
        enemiesToGenerate = getRandomInt(MIN_ENEMIES_PER_ROOM, MAX_ENEMIES_PER_ROOM);

    while (generatedEnemies < enemiesToGenerate) {
        var enemyType = getRandomInt(0, 3);
        console.log('[GENERATING ENEMY] ' + enemyType);
        var validPosition = false,
            spawnPosition = {};

        while (!validPosition) {
            spawnPosition = {
                x: getRandomArbitrary(room.x1, room.x2),
                y: getRandomArbitrary(room.y1, room.y2)
            };
            validPosition = true;
            for (var i = 0; i < this.enemies.length; i++) {
                var enemy = this.enemies[i];
                if (enemy.position.x === spawnPosition.x && enemy.position.y === spawnPosition.y) {
                    validPosition = false;
                    i = this.enemies.length;
                }
            };
        }
        var newEnemy = { type: enemyType, position: spawnPosition };
        console.log('[GENERATED ENEMY]');
        console.log(newEnemy);
        this.enemies.push(newEnemy);

        generatedEnemies++;
    }
}

/**
 * Adds the exit to the last room in the dungeon
 *
 * To make it simpler, the exit will be placed anywhere inside the last room.
 * instead of just on walls.
 */
Map.prototype.generateExitDoor = function() {
    var lastRoom = this.rooms[this.rooms.length - 1],
        randX = getRandomArbitrary(lastRoom.x1 + 1, lastRoom.x2 - 1),
        randY = getRandomArbitrary(lastRoom.y1 + 1, lastRoom.y2 - 1);

    this.exitPosition = { x: randX, y: randY };
}

Map.prototype.print = function() {
    console.log('Printing map');

    console.log('Player spawn: (' + this.playerSpawn.x + ', ' + this.playerSpawn.y + ')');

    for (var x = 0; x < MAX_WIDTH; x++) {
        for (var y = 0; y < MAX_HEIGHT; y++) {
            if (this.terrain[x][y] === WALL) {
                process.stdout.write((this.terrain[x][y] + ' ').bgBlack);
            } else {
                // Print player spawn point for testing..
                if (this.playerSpawn.x === x && this.playerSpawn.y === y) {
                    process.stdout.write((this.terrain[x][y] + ' ').bgGreen);
                } else if (this.exitPosition.x === x && this.exitPosition.y === y) {
                    process.stdout.write((this.terrain[x][y] + ' ').bgBlue);
                } else if (this.treasurePosition.x === x && this.treasurePosition.y === y) {
                    process.stdout.write((this.terrain[x][y] + ' ').bgYellow);
                } else {
                    var hasEnemy = false;
                    for (var i = 0; i < this.enemies.length; i++) {
                        if (this.enemies[i].position.x === x && this.enemies[i].position.y === y) {
                            hasEnemy = true;
                            i = this.enemies.length;
                        }
                    }
                    if (!hasEnemy)
                        process.stdout.write((this.terrain[x][y] + ' ').bgWhite.black);
                    else
                        process.stdout.write((this.terrain[x][y] + ' ').bgRed);
                }
            }
        }
        process.stdout.write('\n');
    }

    // For testing, write terrain array as json to text file
    // var valArray = _.map(this.terrain, function(row) {
    //     return _.map(row, function(tile) {
    //         return tile.type;
    //     });
    // });

    // fs.writeFile('output.txt', JSON.stringify(valArray), function() {
    //     console.log('file saved');
    // });
};

/**
 * Generate random int between a range. (inclusive min and max)
 * @param  {int} min - Minimum
 * @param  {int} max - Maximum
 * @return {int} Random integer between min and max.
 */
function getRandomInt(min, max) {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}

/*
    Generates random int between a range (exclusive max)
 */
function getRandomArbitrary(min, max) {
    return Math.floor(Math.random() * (max - min) + min);
}

module.exports = Map;