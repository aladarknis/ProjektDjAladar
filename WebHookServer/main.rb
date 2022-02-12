require 'sinatra'
require 'json'

set :bind, '0.0.0.0'
root_dir = Dir.pwd

post '/payload' do
  push_json = JSON.parse(request.body.read)
  puts "I got some JSON: #{push_json.inspect}"
  ref = push_json["ref"]
  if ref.include? "main" or ref.include? "master"
    puts `#{root_dir}/update_docker_container.sh >&2`
  end
end

