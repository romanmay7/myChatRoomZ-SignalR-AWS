import { Component, OnInit } from '@angular/core';
import { ChannelService } from '../../services/channel.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit {



  constructor(public channelService: ChannelService) { }
  async ngOnInit()
  {
    await this.channelService.loadChannels();
  }

}
