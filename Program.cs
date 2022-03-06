using System.Text.Json.Serialization;
using System.Text.Json;
string[] lines = System.IO.File.ReadAllLines("Quake.txt");
List<Partida> partidas = new List<Partida>();

Partida partida = new Partida();

int ultimaPartida = 0;
foreach (string line in lines) {
    //Console.WriteLine(line);
    if (line.IndexOf("InitGame")!=-1) {
        ultimaPartida++;
        partida = new Partida();
        partida.game = ultimaPartida;
    } else {
        if(line.IndexOf("Item")!=-1){
            continue;
        }
        if (line.IndexOf("ClientConnect")!=-1) {
            int codigoPartida=int.Parse(line.Substring(21));
            if(partida.status.FindPlayer(codigoPartida)==-1){
                partida.status.players.Add(new Player(codigoPartida));
            }
        } else {
            if (line.IndexOf("ClientUserinfoChanged") != -1) {
                int codigoPlayer = int.Parse(line.Substring(30,line.IndexOf(@" n\")-30));
                
                int comecoNome = line.IndexOf(@"n\")+2;
                int FimNome = line.IndexOf(@"\t\")-comecoNome;
                
                string novoNome = line.Substring(comecoNome,FimNome);

                int indexPlayer = partida.status.FindPlayer(codigoPlayer);

                if (partida.status.players[indexPlayer].nome == "") {
                    partida.status.players[indexPlayer].nome = novoNome;
                } else {
                    if (!partida.status.players[indexPlayer].nome.Equals(novoNome)) {
                        partida.status.players[indexPlayer].old_names.Add(partida.status.players[indexPlayer].nome);
                        partida.status.players[indexPlayer].nome = novoNome;
                    }
                }
            } else {
                if(line.IndexOf("Kill") != -1) {
                    partida.status.total_kills++;
                    int[] variaveisKill = Array.ConvertAll(line.Substring(13,line.IndexOf(":",13)-13).Split(" "), i => int.Parse(i));

                    if (variaveisKill[0] == 1022) {
                        int indexPlayer = partida.status.FindPlayer(variaveisKill[1]);
                        partida.status.players[indexPlayer].kills--;
                    }  else {
                        int indexPlayer = partida.status.FindPlayer(variaveisKill[0]);
                        partida.status.players[indexPlayer].kills++;
                    }
                } else {
                    if (line.IndexOf("ShutdownGame") != -1) {
                        partidas.Add(partida);
                    }
                }
            }
        }
    }
}

var sw = File.CreateText(Path.Combine(Environment.CurrentDirectory,"Quake.json"));

sw.WriteLine(JsonSerializer.Serialize(partidas));

sw.Flush();

class Partida{
    public int game {get;set;}
    public Status status {get;set;}
    public Partida(){
        this.game=0;
        this.status = new Status();
    }
}

class Status{
    public int total_kills {get;set;}
    public List<Player> players {get;set;}
    public Status(){
        this.total_kills =0;
        this.players = new List<Player>();
    }
    public int FindPlayer(int id){
        for(int i = 0; i < players.Count;i++){
            if(players[i].id == id){
                return i;
            }
        }
        return -1;
    }

}
class Player{
    public int id {get;set;}
    public string nome {get;set;}
    public int kills {get;set;}
    public List<string> old_names {get;set;}

    public Player(int id){
        this.id = id;
        this.nome ="";
        this.kills =0;
        this.old_names =new List<string>();
    }
    public Player(){
        this.id = 0;
        this.nome ="";
        this.kills =0;
        this.old_names =new List<string>();
    }
}