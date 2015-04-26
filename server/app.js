var crypto = require('crypto'),
    express = require('express'),
    app = express(),
    bodyParser = require('body-parser'),
    multer = require('multer'),
    _ = require('lodash'),
    Map = require('./map'),
    Player = require('./player');

app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(multer());

app.get('/', function(req, res) {
    res.send();
});

// Store rooms as a hash table where the key is a hash of the unique
// id of player1 and player2. Value will be the actual map object
// Might want to move this to redis or some other data store...
var rooms = {};

/**
 * Returns a room for 2 players.
 * Query string
 *     player_1 - unique id for player 1
 *     player_2 - unique id for player 2
 *
 * unique ids should be the same ones that were sent when creating the room
 */
app.get('/rooms', function(req, res) {
    var player1 = req.query.player_1,
        player2 = req.query.player_2,
        hashKey = crypto.createHash('md5').update(player1 + '_' + player2).digest('hex');
    console.log('find room for ' + player1 + ',' + player2 + ' : ' + hashKey);
    if (rooms[hashKey]) {
        res.send(rooms[hashKey]);
    } else {
        res.status(500).send({ error: 'Room does not exist.' });
    }
});

/**
 * Route to create a new room for 2 players
 * POST data:
 *     player_1 - Unique identifier for player 1
 *     player_2 - Unique identifier for player 2
 */
app.post('/rooms', function(req, res) {
    var player1 = req.body.player_1,
        player2 = req.body.player_2,
        hashKey = crypto.createHash('md5').update(player1 + '_' + player2).digest('hex');
    console.log('Creating room for ' + player1 + ',' + player2 + ' @ ' + hashKey);

    // If the room already exists, just return the room.
    if (!rooms[hashKey]) {
        var playersH = {};
        playersH[player1] = new Player(player1);
        playersH[player2] = new Player(player2);

        rooms[hashKey] = {
            id: hashKey,
            players: playersH,
            isFinished: false,
            map: new Map()
        };
    }

    rooms[hashKey].map.print();

    res.json(rooms[hashKey]);
});

/**
 * Update a specific room
 */
app.post('/rooms/:key', function(req, res) {
    var key = req.params.key,
        playerId = req.body.playerid,
        status = req.body.status,
        score = req.body.score;

    console.log('Updating room: ' + key + ' for ' + playerId);

    if (rooms[key]) {
        if (status > 0 ) {
            // If a player id and status is given, update player's status and score
            if (playerId && status && score) {
                    rooms[key].players[playerId].score = parseInt(score);
                    rooms[key].players[playerId].isFinished = true;

                    // Check if all players in room are finished
                    var allFinished = true;
                    _.each(rooms[key].players, function(player) {
                        if (player.isFinished) {
                            allFinished = false;
                        }
                    });

                    if (allFinished) {
                        rooms[key].isFinished = true;
                    }
                }
        } else {
            delete rooms[key];
            res.json({ 'message' : 'Room finished and deleted'  });
        }

        res.json(rooms[key]);
    } else {
        res.status(500).send({ error: 'Room does not exist.' });
    }
});

app.get('/rooms/:key', function(req, res) {
    var key = req.params.key;
    if (rooms[key]) {
        res.json(rooms[key]);
    } else {
        res.status(500).send({ error: 'Room does not exist.' });
    }
});

var server = app.listen(3000, function() {
    console.log('server loaded');
});
